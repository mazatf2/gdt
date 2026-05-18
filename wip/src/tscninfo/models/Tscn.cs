using System.Collections.Generic;

namespace gdt.wip.tscninfo.models;

public class Tscn(Gd_scene gdScene, List<Ext_resource> extResource, List<Sub_resource> subResource, List<Node> node, List<Connection> connection) {
	public Gd_scene gd_scene = gdScene;
	public List<Ext_resource> ext_resource = extResource;
	public List<Sub_resource> sub_resource = subResource;
	public List<Node> node = node;
	public List<Connection> connection = connection;
}
