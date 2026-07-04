using System.Diagnostics;
using System.Globalization;
using gdt.projects.td.media.npc;
using Godot;
using gdt.shared;
using gdt.shared._3d;

namespace gdt.projects.td;

class State {
	private Camera3D cam;
	private Marker3D lookAt;
}

class Dispose {
	internal Area3D area;
}

[Tool]
public partial class Main : Godot.Control {
	[Export] private Camera3D cam;
	[Export] private Marker3D lookAt;
	private LookAtCam? lookAtCam;

	private Dispose _dispose = new();
	State _state = new State();

	public Main() {
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
	}

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
		Spawner.Spawn((spawnPoint: _spawnPoint, endPoint: _end, npc: _npc, count: count));
	}

	public override void _Ready() {
		_stats = new Stats(128, 16, this);
		_spawnPoint = GetNode<Node3D>("start");
		_end = GetNode<Node3D>("end");
		_npc = GetNode<Node3D>("%fast");
		_navigation = GetNode<NavigationRegion3D>("NavigationRegion3D");
		Debug.Assert(_spawnPoint != null);
		Debug.Assert(_npc != null);
		
		var gridMap = GetNode<GridMap>("%GridMap");
		var areaCon = GetNode<Node3D>("%areaCon");
		areaCon.Position = gridMap.Position;
		areaCon.Position = areaCon.Position with { Y = areaCon.Position.Y + 1 };
		areaCon.TraverseChildren<Node>(i => i.QueueFree());

		var dupes = GetNode<Node3D>("%dupes");
		_dupesObs1 = dupes.GetNode<Node3D>("obs1m");
		_dupesObs1.Position = Vector3.Zero;
		_dupesObs1Preview = dupes.GetNode<Node3D>("obs1m-preview");
		_dupesObs1Preview.Position = Vector3.Zero;

		_dispose.area = new Area3D() {
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

			var pos = t2.Origin;
			var rot = t2.Basis.GetEuler();
			var scale = t2.Basis.Scale;

			//continue;
			var temp = (Node3D)_dispose.area.Duplicate();
			temp.Position = pos;
			areaCon.AddChild(temp);
			temp.Owner = this;

			//Debugger.Break();
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
	private Node3D? _spawnPoint;
	private Node3D? _end;
	private Node3D? _npc;
	private Stats _stats;

	public override void _UnhandledInput(InputEvent _ev) {
		if (_ev is InputEventMouseButton ev) {
			if(ev.Pressed == false){return;}
			var btn = MouseButtonMask.Left;
			if ((ev.ButtonMask & btn) > 0) {
				var hit = Misc.CameraPick((viewport: GetViewport(),
						flags: Project.Physics3DLayer.Placement,
						queryMutate: query
					));
				if (hit == null) { return; }

				var col = hit.Collider.As<Area3D>();
				col.SetMeta("isUse", true);
				var dup = (Node3D)_dupesObs1.Duplicate();

				_navigation.AddChild(dup);
				dup.Owner = this;
				dup.GlobalPosition = col.GlobalPosition;
				_navigation.BakeNavigationMesh();
			}
			else if ((ev.ButtonMask & MouseButtonMask.Right) > 0) {
				var hit = Misc.CameraPick((viewport: GetViewport(),
						flags: Project.Physics3DLayer.Placement,
						queryMutate: query
					));
				if (hit == null) { return; }

				var col = hit.Collider.As<Area3D>();
				col.SetMeta("isUse", true);

				var dup = _dupesObs1Preview;
				dup.GlobalPosition = col.GlobalPosition;
				_stats.NextDrawMode();
			}
			else if (ev.ButtonIndex == MouseButton.WheelDown) {
				Log.TimeStart(out var timeEnd);
				Spawn(3);
				timeEnd("npc", "spawn 3");
			}
			else if (ev.ButtonIndex == MouseButton.WheelUp) {
				Log.TimeStart(out var timeEnd);
				Spawn(6);
				timeEnd("npc", "spawn 6");
			}
		}
		else if (_ev is InputEventMouseMotion ev2) {
		}
		//_ev.Dispose(); //needed?
	}

	public override void _Process(double delta) {
		lookAtCam?.Process(delta);
		_stats?.Process();
		QueueRedraw();
	}

	public override void _Draw() {
		_stats?.Draw();
	}

	public override void _ExitTree() {
		var ids = GetOrphanNodeIds(); //125? --verbose shows no leaks
		var d1 = () => PrintOrphanNodes();
		//Debugger.Break();

		Spawner.ListNpc.ForEach(n => { n.QueueFree(); });
	}
}

class Spawner {
	public static readonly List<Npc> ListNpc = Enumerable.Repeat(new Npc() { CanReUse = true, }, 12).ToList();

	public static void Spawn(in (Node3D spawnPoint, Node3D endPoint, Node3D npc, int count) props /*,in Action<(Npc npc, bool t)> cb*/) {
		for (var i = 0; i < props.count; i++) {
			var npc = ListNpc.Find(n => n.CanReUse);
			if (npc == null) {
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
