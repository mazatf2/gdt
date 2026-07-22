using System.Diagnostics;
using System.Globalization;
using System.Text;
using gdt.projects.td.media.npc;
using Godot;
using gdt.shared;
using gdt.shared._3d;

namespace gdt.projects.td;

class Dispose {
	internal Area3D area;
}

[Tool]
public partial class Main : Godot.Control {
	[Export] private Camera3D cam;
	[Export] private Marker3D lookAt;
	private LookAtCam? lookAtCam;

	private Dispose _dispose = new();

	public override void _EnterTree() {
		lookAtCam = new LookAtCam((cam, lookAt), (props) => {
			cam = props.cam;
			lookAt = props.lookAt;
			AddChild(cam);
			AddChild(lookAt);
			cam.Owner = this;
			lookAt.Owner = this;
		});
	}

	void Spawn(int count) {
		Spawner.Spawn((spawnPoint: _npcSpawnPoint, endPoint: _npcEnd, npc: _npc, count: count));
	}

	public async override void _Ready() {
		_npcSpawnPoint = GetNode<Node3D>("start");
		_npcEnd = GetNode<Node3D>("end");
		_navigation = GetNode<NavigationRegion3D>("NavigationRegion3D");
		Debug.Assert(_npcSpawnPoint != null);

		var gridMap = GetNode<GridMap>("%GridMap");
		var areaCon = GetNode<Node3D>("%areaCon");
		areaCon.Position = gridMap.Position;
		areaCon.Position = areaCon.Position with { Y = areaCon.Position.Y + 1 };
		areaCon.TraverseChildren<Node>(i => i.QueueFree());

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //sometimes needed
		
		var dupes = GetNode<Node3D>("%dupes");
		_dupesObs1 = dupes.GetNode<Node3D>("obs1m");
		_dupesObs1.Position = Vector3.Zero;
		_dupesObs1Preview = dupes.GetNode<Node3D>("obs1m-preview");
		_dupesObs1Preview.Position = Vector3.Zero;
		_npc = dupes.GetNode<Npc>("npc");

		_dispose.area = new Area3D() {
			Name = "placementArea3D",
			CollisionLayer = (uint)Project.Physics3DLayer.Placement,
			CollisionMask = (uint)Project.Physics3DLayer.Placement,
			Children = [
				new CollisionShape3D() {
					Shape = new BoxShape3D() {
						Size = new Vector3(1, 1, 1),
					},
				},
			],
		};

		var meshesArr = gridMap.GetMeshes();

		for (int i = 0; i < meshesArr.Count; i += 2) {
			var transfrom = meshesArr[i].AsTransform3D();
			var mesh = meshesArr[i + 1].As<ArrayMesh>();

			var t2 = transfrom;

			var pos = t2.Pos;
			var rot = t2.Rot;
			var scale = t2.Scale;
			
			var temp = (Node3D)_dispose.area.Duplicate();
			temp.Position = pos;
			areaCon.AddChild(temp);
			temp.Owner = this;
		}

		_dispose.area.QueueFree();
		//_dispose.area.Dispose();
		_areaCon = areaCon;
		_areaCon.TraverseChildren<Node>(i => i.Owner = this);
	}

	private PhysicsRayQueryParameters3D query = new() {
		CollideWithAreas = true,
		CollideWithBodies = false,
	};

	private Node3D _dupesObs1;
	private Node3D _dupesObs1Preview;
	private Node3D _areaCon;
	private NavigationRegion3D? _navigation;
	private Node3D? _npcSpawnPoint;
	private Node3D? _npcEnd;
	private Npc? _npc;

	public override void _UnhandledInput(InputEvent _ev) {
		using var @event = _ev;
		switch (_ev) {
			case InputEventMouseButton ev:
				if (ev.ButtonIndex == MouseButton.Left) {
					var hit = Misc.CameraPick((viewport: GetViewport(),
							flags: Project.Physics3DLayer.Placement,
							queryMutate: query
						));
					if (hit == null) { return; }

					var col = hit.Collider().As<Area3D>();
					col.SetMeta("isUse", true);
					var dup = (Node3D)_dupesObs1.Duplicate();

					_navigation.AddChild(dup);
					dup.Owner = this;
					dup.GlobalPosition = col.GlobalPosition;
					_navigation.BakeNavigationMesh();
				}
				else if (ev.ButtonIndex == MouseButton.Right) {
					var hit = Misc.CameraPick((viewport: GetViewport(),
							flags: Project.Physics3DLayer.Placement,
							queryMutate: query
						));
					if (hit == null) { return; }

					var col = hit.Collider().As<Area3D>();
					col.SetMeta("isUse", true);

					var dup = _dupesObs1Preview;
					dup.GlobalPosition = col.GlobalPosition;
				}
				else if (ev.ButtonIndex == MouseButton.WheelDown) {
					Log.TimeStart(out var timeEnd);
					Spawn(3);
					timeEnd("main:debug", "spawn 3");
				}
				else if (ev.ButtonIndex == MouseButton.WheelUp) {
					Log.TimeStart(out var timeEnd);
					Spawn(6);
					timeEnd("main:debug", "spawn 6");
				}

				if (ev.ButtonIndex == MouseButton.WheelUp && Input.IsPhysicalKeyPressed(Key.F3)) {
					State.Stats.NextDrawMode();
				}

				break;
			case InputEventKey ev: {
				if (ev is { Pressed: true, PhysicalKeycode: Key.F3, ShiftPressed: false, }) {
					State.IsStatsActive.value = !State.IsStatsActive.value;
				}
				else if (ev is { Pressed: true, PhysicalKeycode: Key.F3, ShiftPressed: true, }) {
					State.Stats.NextDrawMode();
				}

				break;
			}
		}
	}

	public override void _Process(double d) {
		var delta = (float)d;
		lookAtCam?.Process(delta);
	}

	public override void _ExitTree() {
		var ids = GetOrphanNodeIds(); //125? --verbose shows no leaks
		var d1 = () => PrintOrphanNodes();
		//Debugger.Break();
		Log.LastCall("main:exit", ids.Count + "", "orphan nodes");

		Spawner.ListNpc.ForEach(n => { n.QueueFree(); });
	}
}

class Spawner {
	public static readonly List<Npc> ListNpc = [];

	public static void SpawnQueue(in Node3D toSpawnNpc, int count) {
	}

	public static void Spawn(in (Node3D spawnPoint, Node3D endPoint, Node3D npc, int count) props /*,in Action<(Npc npc, bool t)> cb*/) {
		if (ListNpc.Count < 20) {
			for (var i = 0; i < 20; i++) {
				var temp = props.npc.Duplicate() as Npc;
				temp.CanReUse = true;
				ListNpc.Add(temp);
			}
		}

		for (var i = 0; i < props.count; i++) {
			var npc = ListNpc.Find(n => {
				return n.CanReUse;
			});

			if (npc == null) {
				Log.LastCall("spawner:spawn", npc?.Name, "is null");
				npc = (Npc)props.npc.Duplicate();
				ListNpc.Add(npc);
			}

			npc.CanReUse = false;

			props.spawnPoint.AddChild(npc);
			npc.Owner = props.spawnPoint;
			npc.GlobalPosition = props.spawnPoint.GlobalPosition;

			npc.NavAgent.TargetPosition = props.endPoint.GlobalPosition;

			//cb((npc, true));
		}
	}
}
