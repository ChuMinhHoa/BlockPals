#!/usr/bin/env python3
"""Generate JellyBlock.shadergraph for Unity URP."""

import json
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Assets" / "BaseGame" / "Shader" / "JellyBlock.shadergraph"

HLSL_BODY = r"""float2 uv = (UV - 0.5) * 2.0;
float2 halfSize = float2(0.82, 0.82);
float2 q = abs(uv) - halfSize + CornerRadius;
float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - CornerRadius;
float aa = 0.012;
float shape = 1.0 - smoothstep(-aa, aa, d);
float outlineOuter = 1.0 - smoothstep(-aa, aa, d - OutlineWidth);
float outlineInner = 1.0 - smoothstep(-aa, aa, d);
float outline = outlineOuter * (1.0 - outlineInner);
float vertGrad = smoothstep(0.0, 1.0, UV.y);
float3 baseCol = lerp(DarkColor.rgb, BaseColor.rgb, vertGrad);
float edgeBevel = smoothstep(-0.18, 0.04, d);
float3 innerCol = lerp(baseCol * 0.55, baseCol, edgeBevel);
float topRim = smoothstep(0.72, 0.98, UV.y) * smoothstep(0.04, -0.08, d);
innerCol = lerp(innerCol, innerCol * 1.12 + float3(0.08, 0.02, 0.02), topRim * 0.45);
float2 hlCenter = float2(-0.38, 0.42);
float2 hlUV = uv - hlCenter;
float angle = 0.35;
float s = sin(angle);
float c = cos(angle);
float2 hlRot = float2(c * hlUV.x - s * hlUV.y, s * hlUV.x + c * hlUV.y);
float2 hq = abs(hlRot) - float2(0.11, 0.27) + 0.075;
float hlDist = length(max(hq, 0.0)) + min(max(hq.x, hq.y), 0.0) - 0.075;
float highlight = (1.0 - smoothstep(-0.02, 0.05, hlDist)) * HighlightIntensity;
highlight *= smoothstep(0.35, 0.85, UV.y);
float topEdge = smoothstep(0.86, 0.96, UV.y) * smoothstep(0.02, -0.04, d);
float3 finalCol = innerCol;
finalCol = lerp(finalCol, float3(1.0, 0.95, 0.95), highlight);
finalCol = lerp(finalCol, finalCol + float3(0.12, 0.08, 0.08), topEdge * 0.35);
finalCol = lerp(OutlineColor.rgb, finalCol, shape);
finalCol = lerp(finalCol, OutlineColor.rgb, outline);
Color = finalCol;
Alpha = max(shape, outline);"""


def gid():
    return uuid.uuid4().hex


def block_node(oid, descriptor, slot_oid):
    return {
        "m_SGVersion": 0,
        "m_Type": "UnityEditor.ShaderGraph.BlockNode",
        "m_ObjectId": oid,
        "m_Group": {"m_Id": ""},
        "m_Name": descriptor,
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": 0, "y": 0, "width": 0, "height": 0}},
        "m_Slots": [{"m_Id": slot_oid}],
        "synonyms": [],
        "m_Precision": 0,
        "m_PreviewExpanded": True,
        "m_DismissedVersion": 0,
        "m_PreviewMode": 0,
        "m_CustomColors": {"m_SerializableColors": []},
        "m_SerializedDescriptor": descriptor,
    }


def property_node(oid, prop_oid, out_slot, x, y):
    return {
        "m_SGVersion": 3,
        "m_Type": "UnityEditor.ShaderGraph.PropertyNode",
        "m_ObjectId": oid,
        "m_Group": {"m_Id": ""},
        "m_Name": "Property",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": x, "y": y, "width": 155, "height": 34}},
        "m_Slots": [{"m_Id": out_slot}],
        "synonyms": [],
        "m_Precision": 0,
        "m_PreviewExpanded": True,
        "m_DismissedVersion": 0,
        "m_PreviewMode": 0,
        "m_CustomColors": {"m_SerializableColors": []},
        "m_Property": {"m_Id": prop_oid},
    }


def main():
    # IDs
    graph = gid()
    cat = gid()
    urp_target = gid()
    urp_unlit = gid()

    props = {
        "base": {"name": "Base Color", "ref": "_BaseColor", "color": [0.92, 0.18, 0.18, 1.0]},
        "dark": {"name": "Dark Color", "ref": "_DarkColor", "color": [0.55, 0.05, 0.08, 1.0]},
        "outline": {"name": "Outline Color", "ref": "_OutlineColor", "color": [0.15, 0.04, 0.04, 1.0]},
        "radius": {"name": "Corner Radius", "ref": "_CornerRadius", "value": 0.22},
        "outline_w": {"name": "Outline Width", "ref": "_OutlineWidth", "value": 0.04},
        "highlight": {"name": "Highlight Intensity", "ref": "_HighlightIntensity", "value": 0.85},
    }

    prop_ids = {k: gid() for k in props}
    prop_node_ids = {k: gid() for k in props}
    prop_out_slots = {k: gid() for k in props}

    uv_node = gid()
    uv_out = gid()
    split_node = gid()
    split_in = gid()
    split_r = gid()
    split_g = gid()
    split_b = gid()
    split_a = gid()
    combine_node = gid()
    combine_in1 = gid()
    combine_in2 = gid()
    combine_out = gid()

    cf = gid()
    cf_slots = {name: gid() for name in [
        "UV", "BaseColor", "DarkColor", "OutlineColor", "CornerRadius", "OutlineWidth", "HighlightIntensity", "Color", "Alpha"
    ]}

    base_block = gid()
    alpha_block = gid()
    base_slot = gid()
    alpha_slot = gid()

    vert_pos = gid()
    vert_norm = gid()
    vert_tan = gid()
    vert_pos_slot = gid()
    vert_norm_slot = gid()
    vert_tan_slot = gid()

    objects = []

    objects.append({
        "m_SGVersion": 3,
        "m_Type": "UnityEditor.ShaderGraph.GraphData",
        "m_ObjectId": graph,
        "m_Properties": [{"m_Id": prop_ids[k]} for k in props],
        "m_Keywords": [],
        "m_Dropdowns": [],
        "m_CategoryData": [{"m_Id": cat}],
        "m_Nodes": [{"m_Id": oid} for oid in [
            vert_pos, vert_norm, vert_tan, uv_node, split_node, combine_node, cf,
            prop_node_ids["base"], prop_node_ids["dark"], prop_node_ids["outline"],
            prop_node_ids["radius"], prop_node_ids["outline_w"], prop_node_ids["highlight"],
            base_block, alpha_block,
        ]],
        "m_GroupDatas": [],
        "m_StickyNoteDatas": [],
        "m_Edges": [
            {"m_OutputSlot": {"m_Node": {"m_Id": uv_node}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": split_node}, "m_SlotId": 0}},
            {"m_OutputSlot": {"m_Node": {"m_Id": split_node}, "m_SlotId": 1}, "m_InputSlot": {"m_Node": {"m_Id": combine_node}, "m_SlotId": 0}},
            {"m_OutputSlot": {"m_Node": {"m_Id": split_node}, "m_SlotId": 2}, "m_InputSlot": {"m_Node": {"m_Id": combine_node}, "m_SlotId": 1}},
            {"m_OutputSlot": {"m_Node": {"m_Id": combine_node}, "m_SlotId": 2}, "m_InputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 0}},
            {"m_OutputSlot": {"m_Node": {"m_Id": prop_node_ids["base"]}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 1}},
            {"m_OutputSlot": {"m_Node": {"m_Id": prop_node_ids["dark"]}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 2}},
            {"m_OutputSlot": {"m_Node": {"m_Id": prop_node_ids["outline"]}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 3}},
            {"m_OutputSlot": {"m_Node": {"m_Id": prop_node_ids["radius"]}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 4}},
            {"m_OutputSlot": {"m_Node": {"m_Id": prop_node_ids["outline_w"]}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 5}},
            {"m_OutputSlot": {"m_Node": {"m_Id": prop_node_ids["highlight"]}, "m_SlotId": 0}, "m_InputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 6}},
            {"m_OutputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 7}, "m_InputSlot": {"m_Node": {"m_Id": base_block}, "m_SlotId": 0}},
            {"m_OutputSlot": {"m_Node": {"m_Id": cf}, "m_SlotId": 8}, "m_InputSlot": {"m_Node": {"m_Id": alpha_block}, "m_SlotId": 0}},
        ],
        "m_VertexContext": {"m_Position": {"x": 0, "y": 0}, "m_Blocks": [{"m_Id": vert_pos}, {"m_Id": vert_norm}, {"m_Id": vert_tan}]},
        "m_FragmentContext": {"m_Position": {"x": 0, "y": 200}, "m_Blocks": [{"m_Id": base_block}, {"m_Id": alpha_block}]},
        "m_PreviewData": {"serializedMesh": {"m_SerializedMesh": "{\"mesh\":{\"instanceID\":0}}", "m_Guid": ""}, "preventRotation": False},
        "m_Path": "BlockPals",
        "m_GraphPrecision": 1,
        "m_PreviewMode": 2,
        "m_OutputNode": {"m_Id": ""},
        "m_SubDatas": [],
        "m_ActiveTargets": [{"m_Id": urp_target}],
    })

    objects.append({"m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.CategoryData", "m_ObjectId": cat, "m_Name": "", "m_ChildObjectList": [{"m_Id": prop_ids[k]} for k in props]})

    for key, data in props.items():
        if "color" in data:
            objects.append({
                "m_SGVersion": 1,
                "m_Type": "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                "m_ObjectId": prop_ids[key],
                "m_Guid": {"m_GuidSerialized": gid()},
                "m_Name": data["name"],
                "m_DefaultRefNameVersion": 1,
                "m_RefNameGeneratedByDisplayName": data["ref"],
                "m_DefaultReferenceName": data["ref"],
                "m_OverrideReferenceName": data["ref"],
                "m_GeneratePropertyBlock": True,
                "m_UseInCustomInterpo": False,
                "m_Precision": 0,
                "m_OverrideHLSLDeclaration": False,
                "m_HLSLDeclarationOverride": 0,
                "m_DefaultValue": {"r": data["color"][0], "g": data["color"][1], "b": data["color"][2], "a": data["color"][3]},
                "m_ColorMode": 0,
            })
            objects.append(property_node(prop_node_ids[key], prop_ids[key], prop_out_slots[key], -920, 150 + list(props.keys()).index(key) * 80))
            c = data["color"]
            objects.append({
                "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": prop_out_slots[key],
                "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out",
                "m_StageCapability": 3, "m_Value": {"x": c[0], "y": c[1], "z": c[2], "w": c[3]},
                "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": [],
            })
        else:
            objects.append({
                "m_SGVersion": 1,
                "m_Type": "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty",
                "m_ObjectId": prop_ids[key],
                "m_Guid": {"m_GuidSerialized": gid()},
                "m_Name": data["name"],
                "m_DefaultRefNameVersion": 1,
                "m_RefNameGeneratedByDisplayName": data["ref"],
                "m_DefaultReferenceName": data["ref"],
                "m_OverrideReferenceName": data["ref"],
                "m_GeneratePropertyBlock": True,
                "m_UseInCustomInterpo": False,
                "m_Precision": 0,
                "m_OverrideHLSLDeclaration": False,
                "m_HLSLDeclarationOverride": 0,
                "m_FloatType": 0,
                "m_RangeValues": {"x": 0.0, "y": 1.0},
                "m_DefaultValue": data["value"],
                "m_NoPrecision": False,
            })
            objects.append(property_node(prop_node_ids[key], prop_ids[key], prop_out_slots[key], -920, 150 + list(props.keys()).index(key) * 80))
            objects.append({
                "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": prop_out_slots[key],
                "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out",
                "m_StageCapability": 3, "m_Value": data["value"], "m_DefaultValue": 0.0, "m_Labels": [],
            })

    # UV
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.UVNode", "m_ObjectId": uv_node,
        "m_Group": {"m_Id": ""}, "m_Name": "UV", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -920, "y": -120, "width": 145, "height": 128}},
        "m_Slots": [{"m_Id": uv_out}], "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": False,
        "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []}, "m_OutputChannel": 0,
    })
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector4MaterialSlot", "m_ObjectId": uv_out,
        "m_Id": 0, "m_DisplayName": "Out", "m_SlotType": 1, "m_Hidden": False, "m_ShaderOutputName": "Out",
        "m_StageCapability": 3, "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_Labels": [],
    })

    # Split + Combine for UV.xy
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.SplitNode", "m_ObjectId": split_node,
        "m_Group": {"m_Id": ""}, "m_Name": "Split", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -720, "y": -120, "width": 120, "height": 149}},
        "m_Slots": [{"m_Id": split_in}, {"m_Id": split_r}, {"m_Id": split_g}, {"m_Id": split_b}, {"m_Id": split_a}],
        "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": False, "m_DismissedVersion": 0, "m_PreviewMode": 0, "m_CustomColors": {"m_SerializableColors": []},
    })
    for sid, name, slot_type in [(split_in, "In", 0), (split_r, "R", 1), (split_g, "G", 1), (split_b, "B", 1), (split_a, "A", 1)]:
        objects.append({
            "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.DynamicVectorMaterialSlot", "m_ObjectId": sid,
            "m_Id": ["In", "R", "G", "B", "A"].index(name), "m_DisplayName": name, "m_SlotType": slot_type,
            "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3,
            "m_Value": {"x": 0, "y": 0, "z": 0, "w": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0, "w": 0},
        })

    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.CombineNode", "m_ObjectId": combine_node,
        "m_Group": {"m_Id": ""}, "m_Name": "Combine", "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -540, "y": -100, "width": 145, "height": 130}},
        "m_Slots": [{"m_Id": combine_in1}, {"m_Id": combine_in2}, {"m_Id": combine_out}],
        "synonyms": [], "m_Precision": 0, "m_PreviewExpanded": False, "m_DismissedVersion": 0, "m_PreviewMode": 0,
        "m_CustomColors": {"m_SerializableColors": []},
    })
    for sid, name, slot_id, slot_type in [(combine_in1, "R", 0, 0), (combine_in2, "G", 1, 0), (combine_out, "Out", 2, 1)]:
        typ = "UnityEditor.ShaderGraph.Vector1MaterialSlot" if slot_type == 0 else "UnityEditor.ShaderGraph.Vector2MaterialSlot"
        obj = {"m_SGVersion": 0, "m_Type": typ, "m_ObjectId": sid, "m_Id": slot_id, "m_DisplayName": name, "m_SlotType": slot_type,
               "m_Hidden": False, "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Labels": []}
        if typ.endswith("Vector2MaterialSlot"):
            obj["m_Value"] = {"x": 0, "y": 0}
            obj["m_DefaultValue"] = {"x": 0, "y": 0}
        else:
            obj["m_Value"] = 0.0
            obj["m_DefaultValue"] = 0.0
        objects.append(obj)

    # Custom Function
    slot_defs = [
        ("UV", 0, 0, "Vector2MaterialSlot"),
        ("BaseColor", 1, 0, "Vector4MaterialSlot"),
        ("DarkColor", 2, 0, "Vector4MaterialSlot"),
        ("OutlineColor", 3, 0, "Vector4MaterialSlot"),
        ("CornerRadius", 4, 0, "Vector1MaterialSlot"),
        ("OutlineWidth", 5, 0, "Vector1MaterialSlot"),
        ("HighlightIntensity", 6, 0, "Vector1MaterialSlot"),
        ("Color", 7, 1, "Vector3MaterialSlot"),
        ("Alpha", 8, 1, "Vector1MaterialSlot"),
    ]
    for name, slot_id, slot_type, typ in slot_defs:
        obj = {"m_SGVersion": 0, "m_Type": f"UnityEditor.ShaderGraph.{typ}", "m_ObjectId": cf_slots[name],
               "m_Id": slot_id, "m_DisplayName": name, "m_SlotType": slot_type, "m_Hidden": False,
               "m_ShaderOutputName": name, "m_StageCapability": 3, "m_Labels": []}
        if typ == "Vector2MaterialSlot":
            obj["m_Value"] = {"x": 0, "y": 0}
            obj["m_DefaultValue"] = {"x": 0, "y": 0}
        elif typ == "Vector3MaterialSlot":
            obj["m_Value"] = {"x": 0, "y": 0, "z": 0}
            obj["m_DefaultValue"] = {"x": 0, "y": 0, "z": 0}
        elif typ == "Vector4MaterialSlot":
            obj["m_Value"] = {"x": 0, "y": 0, "z": 0, "w": 0}
            obj["m_DefaultValue"] = {"x": 0, "y": 0, "z": 0, "w": 0}
        else:
            obj["m_Value"] = 0.0
            obj["m_DefaultValue"] = 0.0
        objects.append(obj)

    objects.append({
        "m_SGVersion": 1,
        "m_Type": "UnityEditor.ShaderGraph.CustomFunctionNode",
        "m_ObjectId": cf,
        "m_Group": {"m_Id": ""},
        "m_Name": "JellyBlock (Custom Function)",
        "m_DrawState": {"m_Expanded": True, "m_Position": {"serializedVersion": "2", "x": -240, "y": -80, "width": 290, "height": 280}},
        "m_Slots": [{"m_Id": cf_slots[n]} for n, *_ in slot_defs],
        "synonyms": ["code", "HLSL"],
        "m_Precision": 0,
        "m_PreviewExpanded": True,
        "m_DismissedVersion": 0,
        "m_PreviewMode": 0,
        "m_CustomColors": {"m_SerializableColors": []},
        "m_SourceType": 1,
        "m_FunctionName": "JellyBlock_float",
        "m_FunctionSource": "",
        "m_FunctionBody": HLSL_BODY,
    })

    # Output blocks
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.ColorRGBMaterialSlot", "m_ObjectId": base_slot,
        "m_Id": 0, "m_DisplayName": "Base Color", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "BaseColor",
        "m_StageCapability": 2, "m_Value": {"x": 0.5, "y": 0.5, "z": 0.5}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0},
        "m_Labels": [], "m_ColorMode": 0, "m_DefaultColor": {"r": 0.5, "g": 0.5, "b": 0.5, "a": 1.0},
    })
    objects.append(block_node(base_block, "SurfaceDescription.BaseColor", base_slot))
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.Vector1MaterialSlot", "m_ObjectId": alpha_slot,
        "m_Id": 0, "m_DisplayName": "Alpha", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Alpha",
        "m_StageCapability": 2, "m_Value": 1.0, "m_DefaultValue": 1.0, "m_Labels": [],
    })
    objects.append(block_node(alpha_block, "SurfaceDescription.Alpha", alpha_slot))

    # Vertex blocks
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.PositionMaterialSlot", "m_ObjectId": vert_pos_slot,
        "m_Id": 0, "m_DisplayName": "Position", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Position",
        "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0,
    })
    objects.append(block_node(vert_pos, "VertexDescription.Position", vert_pos_slot))
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.NormalMaterialSlot", "m_ObjectId": vert_norm_slot,
        "m_Id": 0, "m_DisplayName": "Normal", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Normal",
        "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0,
    })
    objects.append(block_node(vert_norm, "VertexDescription.Normal", vert_norm_slot))
    objects.append({
        "m_SGVersion": 0, "m_Type": "UnityEditor.ShaderGraph.TangentMaterialSlot", "m_ObjectId": vert_tan_slot,
        "m_Id": 0, "m_DisplayName": "Tangent", "m_SlotType": 0, "m_Hidden": False, "m_ShaderOutputName": "Tangent",
        "m_StageCapability": 1, "m_Value": {"x": 0, "y": 0, "z": 0}, "m_DefaultValue": {"x": 0, "y": 0, "z": 0}, "m_Labels": [], "m_Space": 0,
    })
    objects.append(block_node(vert_tan, "VertexDescription.Tangent", vert_tan_slot))

    # URP target
    objects.append({"m_SGVersion": 2, "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalUnlitSubTarget", "m_ObjectId": urp_unlit})
    objects.append({
        "m_SGVersion": 1,
        "m_Type": "UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget",
        "m_ObjectId": urp_target,
        "m_Datas": [],
        "m_ActiveSubTarget": {"m_Id": urp_unlit},
        "m_AllowMaterialOverride": False,
        "m_SurfaceType": 1,
        "m_ZTestMode": 4,
        "m_ZWriteControl": 0,
        "m_AlphaMode": 0,
        "m_RenderFace": 2,
        "m_AlphaClip": True,
        "m_CastShadows": False,
        "m_ReceiveShadows": False,
        "m_DisableTint": False,
        "m_AdditionalMotionVectorMode": 0,
        "m_AlembicMotionVectors": False,
        "m_SupportsLODCrossFade": False,
        "m_CustomEditorGUI": "",
        "m_SupportVFX": False,
    })

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text("\n\n".join(json.dumps(o, indent=4) for o in objects), encoding="utf-8")
    print(f"Generated {OUTPUT}")


if __name__ == "__main__":
    main()
