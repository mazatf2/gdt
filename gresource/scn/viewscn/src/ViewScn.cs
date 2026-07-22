using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using gdt.gresource.scn.viewscn.godot;
using Godot;
using static gdt.gresource.scn.viewscn.godot.VARIANT;

namespace gdt.gresource.scn.viewscn;

//some parts are from godot engine git

public class ViewScn {
	public static void ParseHeader() {
		using var stream = File.Open("D:/ga/gdt/gresource/scn/viewscn/viewscn.tests/data/scene2.scn.nocompression", FileMode.Open);
		using var reader = new BinaryReader(stream, encoding: new UTF8Encoding());
		var v = new ViewScn();
		v.Parse(reader);
	}

	string get_unicode_string(in BinaryReader r) {
		int len = (int)r.ReadUInt32(); //godot uint32 -> int
		var arr = r.ReadBytes(len);

		if (arr.Length != len) {
			Debugger.Break();
			throw new Exception("t2.Length != len");
		}

		return string.Join(string.Empty, arr.Select(i => (char)i));
	}

	string _get_string(in BinaryReader r) {
		UInt32 id = r.ReadUInt32();
		if ((id & 0x80000000) != 0) {
			UInt32 len = id & 0x7FFFFFFF;

			if (len == 0) {
				return string.Empty;
			}

			var arr = r.ReadBytes((int)len);

			return string.Join(string.Empty, arr.Select(i => (char)i));
		}

		return string_map[(int)id];
	}

	public void Parse(in BinaryReader r) {
		var header = r.ReadBytes(4);
		if (header[0] == 'R' &&
			header[1] == 'S' &&
			header[2] == 'C' &&
			header[3] == 'C') {
			var cmode = (Compression.Mode)r.ReadUInt32();
			Console.Out.WriteLine($"compressed {cmode}");
			Debugger.Break();
		}
		else if (header[0] != 'R' &&
				header[1] != 'S' &&
				header[2] != 'R' &&
				header[3] != 'C') {
			Debugger.Break();
			throw new InvalidDataException();
		}

		uint big_endian = r.ReadUInt32(); //get_32()
		uint use_real64 = r.ReadUInt32();

		f f;
		if (big_endian != 0) {
			f = new fBig();
		}
		else {
			f = new fLittle();
		}

		uint ver_major = r.ReadUInt32();
		uint ver_minor = r.ReadUInt32();
		ver_format = r.ReadUInt32();

		type = get_unicode_string(r);
		importmd_ofs = f.get_64(r);
		var flags = f.get_32(r);

		if ((flags & (uint)ResourceFormatSaverBinaryInstance.FORMAT_FLAG_NAMED_SCENE_IDS) != 0) {
			using_named_scene_ids = true;
		}

		if ((flags & (uint)ResourceFormatSaverBinaryInstance.FORMAT_FLAG_UIDS) != 0) {
			using_uids = true;
		}

		real_is_double = (flags & (uint)ResourceFormatSaverBinaryInstance.FORMAT_FLAG_REAL_T_IS_DOUBLE) != 0;

		if (using_uids) {
			uid = f.get_64(r);
		}
		else {
			f.get_64(r);
			uid = null;
		}

		if ((flags & (uint)ResourceFormatSaverBinaryInstance.FORMAT_FLAG_HAS_SCRIPT_CLASS) != 0) {
			script_class = get_unicode_string(r);
		}

		for (var i = 0; i < (int)ResourceFormatSaverBinaryInstance.RESERVED_FIELDS; i++) {
			var t2 = f.get_32(r);
		}

		var string_table_Size = f.get_32(r);
		//string_map = new string[string_table_Size]; //null terminated

		for (var i = 0; i < string_table_Size; i++) {
			var s = get_unicode_string(r);
			string_map.Add(s);
		}

		var ext_resources_Size = f.get_32(r);
		for (var i = 0; i < ext_resources_Size; i++) {
			ExtResource er = new() {
				type = get_unicode_string(r),
				path = get_unicode_string(r),
			};
			if (using_uids) {
				er.uid = f.get_64(r);
			}

			external_resources.Add(er);
		}

		var int_resources_Size = f.get_32(r);

		for (var i = 0; i < int_resources_Size; i++) {
			IntResource ir = new() {
				path = get_unicode_string(r),
				offset = f.get_64(r),
			};

			internal_resources.Add(ir);
		}

		foreach (var internalRes in internal_resources) {
			r.BaseStream.Seek((long)internal_resources[0].offset, SeekOrigin.Begin);
			var currentClass = get_unicode_string(r);
			int pc = (int)f.get_32(r); //
			var temp = new ResourceCacheEntry() {
				Type = currentClass,
				InternalRes = internalRes,
			};
			ResourceCache.Add(temp);

			for (int i = 0; i < pc; i++) {
				var name = _get_string(r);
				var pos = r.BaseStream.Position;
				var value = parse_variant(f, r);
				temp.Data[name] = value;
			}
		}
	}

	private Godot.Variant parse_variant(in f f, in BinaryReader r) {
		var prop_type = (VARIANT)f.get_32(r);
		Godot.Variant? r_v = null;
		switch (prop_type) {
			case VARIANT_NIL: {
				r_v = new Godot.Variant();
				break;
			}

			case VARIANT_BOOL: {
				r_v = f.get_32(r) == 1;
				break;
			}

			case VARIANT_INT: {
				r_v = f.get_32(r);
				break;
			}

			case VARIANT_INT64: {
				r_v = f.get_64(r);
				break;
			}

			case VARIANT_FLOAT: {
				r_v = real_is_double ? f.get_64(r) : f.get_32(r);
				break;
			}

			case VARIANT_DOUBLE: {
				r_v = f.get_64(r);
				break;
			}

			case VARIANT_STRING: {
				r_v = get_unicode_string(r);
				break;
			}

			case VARIANT_VECTOR2: {
				Godot.Vector2 v;
				v.X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Y = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_VECTOR2I: {
				Godot.Vector2I v;
				v.X = (int)f.get_32(r); //
				v.Y = (int)f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_RECT2: {
				Rect2 v = new();
				v.Position = v.Position with { X = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Position = v.Position with { Y = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Size = v.Size with { X = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Size = v.Size with { Y = real_is_double ? f.get_64(r) : f.get_32(r) };
				r_v = v;
				break;
			}

			case VARIANT_RECT2I: {
				Rect2I v = new();
				v.Position = v.Position with { X = (int)f.get_32(r) };
				v.Position = v.Position with { Y = (int)f.get_32(r) };
				v.Size = v.Size with { X = (int)f.get_32(r) };
				v.Size = v.Size with { Y = (int)f.get_32(r) };
				r_v = v;
				break;
			}

			case VARIANT_VECTOR3: {
				Godot.Vector3 v;
				v.X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Z = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_VECTOR3I: {
				Vector3I v;
				v.X = (int)f.get_32(r);
				v.Y = (int)f.get_32(r);
				v.Z = (int)f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_VECTOR4: {
				Godot.Vector4 v;
				v.X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.W = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_VECTOR4I: {
				Vector4I v;
				v.X = (int)f.get_32(r);
				v.Y = (int)f.get_32(r);
				v.Z = (int)f.get_32(r);
				v.W = (int)f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_PLANE: {
				Godot.Plane v = new();
				v.Normal = v.Normal with { X = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Normal = v.Normal with { Y = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Normal = v.Normal with { Z = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.D = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_QUATERNION: {
				Godot.Quaternion v;
				v.X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.W = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_AABB: {
				Aabb v = new();
				v.Position = v.Position with { X = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Position = v.Position with { Y = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Position = v.Position with { Z = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Size = v.Size with { X = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Size = v.Size with { Y = real_is_double ? f.get_64(r) : f.get_32(r) };
				v.Size = v.Size with { Z = real_is_double ? f.get_64(r) : f.get_32(r) };
				r_v = v;
				break;
			}

			case VARIANT_TRANSFORM2D: {
				/*
				Transform2D v = new();
				v.Columns.X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[0].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[1].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[1].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[2].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[2].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				*/
				break;
			}

			case VARIANT_BASIS: {
				/*
				Basis v = new();
				v.Rows[0].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[0].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[0].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[1].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[1].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[1].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[2].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[2].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Rows[2].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
	*/
				break;
			}

			case VARIANT_TRANSFORM3D: {
				/*
				Transform3D v = new();
				v.Basis.Rows[0].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[0].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[0].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[1].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[1].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[1].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[2].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[2].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Basis.Rows[2].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Origin.X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Origin.Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Origin.Z = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				*/
				break;
			}

			case VARIANT_PROJECTION: {
				/*
				Projection v = new();
				v.Columns[0].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[0].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[0].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[0].W = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[1].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[1].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[1].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[1].W = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[2].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[2].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[2].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[2].W = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[3].X = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[3].Y = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[3].Z = real_is_double ? f.get_64(r) : f.get_32(r);
				v.Columns[3].W = real_is_double ? f.get_64(r) : f.get_32(r);
				r_v = v;
				*/
				break;
			}

			case VARIANT_COLOR: {
				Color v; // Colors should always be in single-precision.
				v.R = f.get_32(r);
				v.G = f.get_32(r);
				v.B = f.get_32(r);
				v.A = f.get_32(r);
				r_v = v;
				break;
			}

			case VARIANT_STRING_NAME: {
				r_v = new StringName(get_unicode_string(r));
				break;
			}


			case VARIANT_NODE_PATH: {
				/*
				Vector<StringName> names;
				Vector<StringName> subnames;
				bool absolute;

				int name_count = f.get_16(r);
				var subname_count = f.get_16(r);
				absolute = subname_count & 0x8000;
				subname_count &= 0x7FFF;
				if (ver_format < FORMAT_VERSION_NO_NODEPATH_PROPERTY) {
					subname_count += 1; // has a property field, so we should count it as well
				}

				for (int i = 0; i < name_count; i++) {
					names.push_back(_get_string());
				}
				for (UInt32 i = 0; i < subname_count; i++) {
					subnames.push_back(_get_string());
				}

				NodePath np = NodePath(names, subnames, absolute);

				r_v = np;
				*/
				break;
			}

			case VARIANT_RID: {
				r_v = f.get_32(r);
				break;
			}

			case VARIANT_OBJECT: {
				VARIANT objtype = (VARIANT)f.get_32(r);

				switch (objtype) {
					case OBJECT_EMPTY: {
						//do none
					}
						break;
					case OBJECT_INTERNAL_RESOURCE: {
						UInt32 index = f.get_32(r);
						String path;

						if (using_named_scene_ids) {
							// New format.
							//ERR_FAIL_INDEX_V((int)index, internal_resources.size(), ERR_PARSE_ERROR);
							//path = internal_resources[(int)index].path;
						}
						else {
							//path += res_path + "::" + itos(index);
						}

						//always use internal cache for loading internal resources
						//if (!internal_index_cache.has(path)) {
						//	WARN_PRINT(vformat("Couldn't load resource (no cache): %s.", path));
						//	r_v = Variant();
						//}
						//else {
						//	r_v = internal_index_cache[path];
						//}
					}
						break;
					case OBJECT_EXTERNAL_RESOURCE: {
						//old file format, still around for compatibility

						String exttype = get_unicode_string(r);
						String path = get_unicode_string(r);
/*
						if (!path.contains("://") && path.is_relative_path()) {
							// path is relative to file being loaded, so convert to a resource path
							path = ProjectSettings::get_singleton()->localize_path(res_path.get_base_dir().path_join(path));
						}

						if (remaps.find(path)) {
							path = remaps[path];
						}

						Ref<Resource> res = ResourceLoader::load(path, exttype, cache_mode_for_external);

						if (res.is_null()) {
							WARN_PRINT(vformat("Couldn't load resource: %s.", path));
						}
*/
						r_v = "OBJECT_EXTERNAL_RESOURCE," + exttype + "," + path;
					}
						break;
					case OBJECT_EXTERNAL_RESOURCE_INDEX: {
						//new file format, just refers to an index in the external list
						int erindex = (int)f.get_32(r);
						r_v = "OBJECT_EXTERNAL_RESOURCE_INDEX," + erindex;
						//if (erindex < 0 || erindex >= external_resources.size()) {
						//WARN_PRINT("Broken external resource! (index out of size)");
						//	r_v = Variant();
						//}
						//else {
						/*
						Ref<ResourceLoader::LoadToken> & load_token = external_resources.write[erindex].load_token;
						if (load_token.is_valid()) {
							// If not valid, it's OK since then we know this load accepts broken dependencies.
							Error err;
							Ref<Resource> res = ResourceLoader::_load_complete(*load_token.ptr(), &err);
							if (res.is_null()) {
								if (!ResourceLoader::is_cleaning_tasks()) {
									if (!ResourceLoader::get_abort_on_missing_resources()) {
										ResourceLoader::notify_dependency_error(local_path, external_resources[erindex].path, external_resources[erindex].type);
									}
									else {
										error = ERR_FILE_MISSING_DEPENDENCIES;
										ERR_FAIL_V_MSG(error, vformat("Can't load dependency: '%s'.", external_resources[erindex].path));
									}
								}
							}
							else {
								r_v = res;
							}
						}
						*/
						//}
					}
						break;
					default: {
						//ERR_FAIL_V(ERR_FILE_CORRUPT);
						throw new Exception();
					}
						break;
				}

				break;
			}

			case VARIANT_CALLABLE:
				r_v = new Callable();
				break;

			case VARIANT_SIGNAL:
				r_v = new Signal();
				break;

			case VARIANT_DICTIONARY: {
				UInt32 len = f.get_32(r);
				Godot.Collections.Dictionary d = new(); //last bit means shared
				len &= 0x7FFFFFFF;
				for (UInt32 i = 0; i < len; i++) {
					Variant key = parse_variant(in f, in r);
					//Error err = parse_variant(in f, in r, out key);

					//ERR_FAIL_COND_V_MSG(err, ERR_FILE_CORRUPT, "Error when trying to parse Variant.");
					Variant value = parse_variant(in f, in r);
					//err = parse_variant(value);

					//ERR_FAIL_COND_V_MSG(err, ERR_FILE_CORRUPT, "Error when trying to parse Variant.");
					d[key] = value;
				}

				r_v = d;
				break;
			}
			case VARIANT_ARRAY: {
				UInt32 len = f.get_32(r);
				//Array a; //last bit means shared
				//List<Godot.Variant> a = [];// = [];
				Godot.Collections.Array<Godot.Variant> a = new();
				len &= 0x7FFFFFFF;
				//a.resize(len);
				for (UInt32 i = 0; i < len; i++) {
					Variant val = parse_variant(f, r);
					//ERR_FAIL_COND_V_MSG(err, ERR_FILE_CORRUPT, "Error when trying to parse Variant.");
					//a[i] = val;
					a.Add(val);
				}

				r_v = a;
			}
				break;
			case VARIANT_PACKED_BYTE_ARRAY: {
				/*
				UInt32 len = f.get_32(r);

				Vector<uint8_t> array;
				array.resize(len);
				uint8_t* w = array.ptrw();
				f->get_buffer(w, len);
				_advance_padding(len);

				r_v = array;*/
				break;
			}

			case VARIANT_PACKED_INT32_ARRAY: {
				UInt32 len = f.get_32(r);

				//Vector<int32_t> array;
				//List<Int32> array = new();
				//array.resize(len);
				//int32_t* w = array.ptrw();
				//f->get_buffer((uint8_t*)w, len * sizeof(int32_t));
				var arr = r.ReadBytes((int)(len * 4));
				/*#ifdef BIG_ENDIAN_ENABLED
								{
									UInt32* ptr = (UInt32*)w.ptr();
									for (int i = 0; i < len; i++) {
										ptr[i] = BSWAP32(ptr[i]);
									}
								}

				#endif*/

				r_v = arr;
				break;
			}

			case VARIANT_PACKED_INT64_ARRAY: {
				/*
					UInt32 len = f.get_32(r);

					Vector<int64_t> array;
					array.resize(len);
					int64_t* w = array.ptrw();
					f->get_buffer((uint8_t*)w, len * sizeof(int64_t));
	#ifdef BIG_ENDIAN_ENABLED
					{
						uint64_t* ptr = (uint64_t*)w.ptr();
						for (int i = 0; i < len; i++) {
							ptr[i] = BSWAP64(ptr[i]);
						}
					}

	#endif

					r_v = array;*/
				break;
			}

			case VARIANT_PACKED_FLOAT32_ARRAY: {
				/*
				UInt32 len = f.get_32(r);

				Vector<float> array;
				array.resize(len);
				float* w = array.ptrw();
				f->get_buffer((uint8_t*)w, len * sizeof(float));
#ifdef BIG_ENDIAN_ENABLED
				{
					UInt32* ptr = (UInt32*)w.ptr();
					for (int i = 0; i < len; i++) {
						ptr[i] = BSWAP32(ptr[i]);
					}
				}

#endif

				r_v = array;
			*/
				break;
			}

			case VARIANT_PACKED_FLOAT64_ARRAY: {
				/*
				UInt32 len = f.get_32(r);

				Vector<double> array;
				array.resize(len);
				double* w = array.ptrw();
				f->get_buffer((uint8_t*)w, len * sizeof(double));
#ifdef BIG_ENDIAN_ENABLED
				{
					uint64_t* ptr = (uint64_t*)w.ptr();
					for (int i = 0; i < len; i++) {
						ptr[i] = BSWAP64(ptr[i]);
					}
				}

#endif

				r_v = array;
			}*/
				break;
			}
			case VARIANT_PACKED_STRING_ARRAY: {
				UInt32 len = f.get_32(r);
				List<string> array = [];
				for (UInt32 i = 0; i < len; i++) {
					array.Add(get_unicode_string(r));
				}

				r_v = array.ToArray();

				break;
			}

			case VARIANT_PACKED_VECTOR2_ARRAY: {
				/*
					UInt32 len = f.get_32(r);
					var arr = r.ReadBytes((int)(len*2));
					r_v = array.ToArray();

					Vector<Vector2> array;
					array.resize(len);
					Vector2* w = array.ptrw();
					static_assert(sizeof(Vector2) == 2 * sizeof(real_t));
					const Error err = read_reals(reinterpret_cast<real_t*>(w), f, len * 2);
					ERR_FAIL_COND_V(err != OK, err);

					r_v = array;*/
				break;
			}

			case VARIANT_PACKED_VECTOR3_ARRAY: {
				/*
					UInt32 len = f.get_32(r);

					Vector<Vector3> array;
					array.resize(len);
					Vector3* w = array.ptrw();
					static_assert(sizeof(Vector3) == 3 * sizeof(real_t));
					const Error err = read_reals(reinterpret_cast<real_t*>(w), f, len * 3);
					ERR_FAIL_COND_V(err != OK, err);

					r_v = array;*/
				break;
			}

			case VARIANT_PACKED_COLOR_ARRAY: {
				/*
					UInt32 len = f.get_32(r);

					Vector<Color> array;
					array.resize(len);
					Color* w = array.ptrw();
					// Colors always use `float` even with double-precision support enabled
					static_assert(sizeof(Color) == 4 * sizeof(float));
					f->get_buffer((uint8_t*)w, len * sizeof(float) * 4);
	#ifdef BIG_ENDIAN_ENABLED
					{
						UInt32* ptr = (UInt32*)w.ptr();
						for (int i = 0; i < len * 4; i++) {
							ptr[i] = BSWAP32(ptr[i]);
						}
					}

	#endif

					r_v = array;*/
				break;
			}

			case VARIANT_PACKED_VECTOR4_ARRAY: {
				/*
					UInt32 len = f.get_32(r);

					Vector<Vector4> array;
					array.resize(len);
					Vector4* w = array.ptrw();
					static_assert(sizeof(Vector4) == 4 * sizeof(real_t));
					const Error err = read_reals(reinterpret_cast<real_t*>(w), f, len * 4);
					ERR_FAIL_COND_V(err != OK, err);

					r_v = array;
				}*/
				break;


				//ERR_FAIL_V(ERR_FILE_CORRUPT);
			}
			default: {
				throw new ArgumentOutOfRangeException();
			}
		}

		//Debugger.Break();

		if (r_v == null) {
			throw new ArgumentOutOfRangeException();
		}


		return (Variant)r_v; //
	}

	List<ResourceCacheEntry> ResourceCache = [];

	public List<string> string_map { get; set; } = [];

	public List<IntResource> internal_resources { get; set; } = [];

	public List<ExtResource> external_resources { get; set; } = [];

	public ulong importmd_ofs { get; set; }

	public string type { get; set; }

	public uint ver_format { get; set; }

	public bool real_is_double { get; set; }

	public string script_class { get; set; }

	public UInt64? uid { get; set; }

	public bool using_named_scene_ids { get; set; }

	public bool using_uids { get; set; }
}

public static class BinaryReaderBigEndianExtensions {
	public static short ReadInt16BigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadInt16BigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(2, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[2]));

	public static ushort ReadUInt16BigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadUInt16BigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(2, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[2]));

	public static int ReadInt32BigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadInt32BigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(4, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[4]));

	public static uint ReadUInt32BigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadUInt32BigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(4, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[4]));

	public static long ReadInt64BigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadInt64BigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(8, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[8]));

	public static ulong ReadUInt64BigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadUInt64BigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(8, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[8]));

	public static Half ReadHalfBigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadHalfBigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(2, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[2]));

	public static float ReadSingleBigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadSingleBigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(4, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[4]));

	public static double ReadDoubleBigEndian(this BinaryReader binaryReader) => BinaryPrimitives.ReadDoubleBigEndian(
		binaryReader.BaseStream is MemoryStream ms && ms.TryReadSpanUnsafe(8, out ReadOnlySpan<byte> readBytes) ? readBytes : binaryReader.ReadSpan(stackalloc byte[8]));

	private static ReadOnlySpan<byte> ReadSpan(this BinaryReader binaryReader, Span<byte> buffer) {
		binaryReader.BaseStream.ReadExactly(buffer);
		return buffer;
	}

	private static bool TryReadSpanUnsafe(this MemoryStream memoryStream, int numBytes, out ReadOnlySpan<byte> readBytes) {
		if (memoryStream.TryGetBuffer(out var msBuffer)) {
			readBytes = msBuffer.AsSpan((int)memoryStream.Position, numBytes);
			memoryStream.Seek(numBytes, SeekOrigin.Current);
			return true;
		}
		else {
			readBytes = [];
			return false;
		}
	}
}

interface f {
	public abstract byte get_8(in BinaryReader r);
	public abstract UInt16 get_16(in BinaryReader r);
	public abstract UInt32 get_32(in BinaryReader r);
	public abstract UInt64 get_64(in BinaryReader r);
}

class fLittle : f {
	public byte get_8(in BinaryReader r) => r.ReadByte();

	//ushort
	public UInt16 get_16(in BinaryReader r) => r.ReadUInt16();

	//uint
	public UInt32 get_32(in BinaryReader r) => r.ReadUInt32();

	//ulong
	public UInt64 get_64(in BinaryReader r) => r.ReadUInt64();
}

class fBig : f {
	public byte get_8(in BinaryReader r) => r.ReadByte();
	public UInt16 get_16(in BinaryReader r) => r.ReadUInt16BigEndian();
	public UInt32 get_32(in BinaryReader r) => r.ReadUInt32BigEndian();
	public UInt64 get_64(in BinaryReader r) => r.ReadUInt64BigEndian();
}

class ResourceCacheEntry {
	public IntResource InternalRes { get; set; }
	public Dictionary<string, Godot.Variant> Data { get; set; } = new();
	public string Type { get; set; }
}
