using System;

namespace gdt.gresource.scn.viewscn.godot;

//some parts are from godot engine git

public enum ResourceFormatSaverBinaryInstance : System.UInt32 {
	FORMAT_FLAG_NAMED_SCENE_IDS = 1,
	FORMAT_FLAG_UIDS = 2,
	FORMAT_FLAG_REAL_T_IS_DOUBLE = 4,
	FORMAT_FLAG_HAS_SCRIPT_CLASS = 8,

	// Amount of reserved 32-bit fields in resource header
	RESERVED_FIELDS = 11
};

public struct ExtResource {
	public String path;
	public String type;

	public UInt64? uid;
	//Ref<ResourceLoader::LoadToken> load_token;
};

public struct IntResource {
	public String path;
	public UInt64 offset;
};
