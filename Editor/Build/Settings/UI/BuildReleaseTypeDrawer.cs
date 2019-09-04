using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

namespace SuperSystems.UnityBuild
{

[CustomPropertyDrawer(typeof(BuildReleaseType))]
public class BuildReleaseTypeDrawer : PropertyDrawer
{
	private Dictionary<string, ReorderableList> vrSDKLists;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Limit valid characters.
        // TODO: This might not be necessary since name will need to be sanitized for different needs later (as an enum entry, pre-processor define, etc.)
        //char chr = Event.current.character;
        //if ((chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9') && chr != '-' && chr != '_' && chr != ' ')
        //{
        //    Event.current.character = '\0';
        //}

        bool show = property.isExpanded;
        UnityBuildGUIUtility.DropdownHeader(property.FindPropertyRelative("typeName").stringValue, ref show, false);
        property.isExpanded = show;

        if (show)
        {
            EditorGUILayout.BeginVertical(UnityBuildGUIUtility.dropdownContentStyle);

            GUILayout.Label("Basic Info", UnityBuildGUIUtility.midHeaderStyle);

            SerializedProperty typeName = property.FindPropertyRelative("typeName");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type Name");
            typeName.stringValue = BuildProject.SanitizeFolderName(GUILayout.TextArea(typeName.stringValue));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(property.FindPropertyRelative("bundleIndentifier"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("productName"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("companyName"));

            GUILayout.Space(20);
            GUILayout.Label("Build Options", UnityBuildGUIUtility.midHeaderStyle);

            EditorGUILayout.PropertyField(property.FindPropertyRelative("customDefines"));

            SerializedProperty developmentBuild = property.FindPropertyRelative("developmentBuild");
            SerializedProperty allowDebugging = property.FindPropertyRelative("allowDebugging");
            SerializedProperty enableHeadlessMode = property.FindPropertyRelative("enableHeadlessMode");
			SerializedProperty enableVR = property.FindPropertyRelative("virtualRealitySupported");
			SerializedProperty supportedSDKs = property.FindPropertyRelative("virtualRealitySDKs");

			if (vrSDKLists == null)
				vrSDKLists = new Dictionary<string, ReorderableList>();

			ReorderableList vrSDKList;
			if (!vrSDKLists.TryGetValue(property.propertyPath, out vrSDKList))
			{
				vrSDKList = CreateVRSDKList(supportedSDKs);
				vrSDKLists.Add(property.propertyPath, vrSDKList);
			}

            EditorGUI.BeginDisabledGroup(enableHeadlessMode.boolValue);
            developmentBuild.boolValue = EditorGUILayout.ToggleLeft(" Development Build", developmentBuild.boolValue);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!developmentBuild.boolValue);
            allowDebugging.boolValue = EditorGUILayout.ToggleLeft(" Script Debugging", allowDebugging.boolValue);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(developmentBuild.boolValue);
            enableHeadlessMode.boolValue = EditorGUILayout.ToggleLeft(" Headless Mode", enableHeadlessMode.boolValue);
            EditorGUI.EndDisabledGroup();

			enableVR.boolValue = EditorGUILayout.ToggleLeft(" Virtual Reality Supported", enableVR.boolValue);
			if (enableVR.boolValue)
			{
				vrSDKList.DoLayoutList();
			}

			EditorGUILayout.PropertyField(property.FindPropertyRelative("sceneList"));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete", GUILayout.MaxWidth(150)))
            {
                BuildReleaseType[] types = BuildSettings.releaseTypeList.releaseTypes;
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].typeName == property.FindPropertyRelative("typeName").stringValue)
                    {
                        ArrayUtility.RemoveAt<BuildReleaseType>(ref BuildSettings.releaseTypeList.releaseTypes, i);
                        GUIUtility.keyboardControl = 0;
                        break;
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            property.serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();
        }

        EditorGUI.EndProperty();
    }

	private ReorderableList CreateVRSDKList(SerializedProperty property) {
		ReorderableList list = new ReorderableList(property.serializedObject, property, true, true, true, true);
		list.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Supported Virtual Reality SDKs");
		};
		list.onCanRemoveCallback = (x) => { return list.count > 0; };
		list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.stringValue);
			//EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
		};
		list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
			var menu = new GenericMenu();
			HashSet<string> sdks = new HashSet<string>();
			foreach(BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
				foreach(var sdk in PlayerSettings.GetVirtualRealitySDKs(targetGroup))
					sdks.Add(sdk);

			foreach(var sdk in sdks) {
				menu.AddItem(new GUIContent(sdk), false, sdkAddHandler, new object[] { l, sdk });
			}
			menu.ShowAsContext();
		};

		return list;
	}

	private void sdkAddHandler(object target) {
		var data = (object[])target;
		var vrSDKList = data[0] as ReorderableList;
		var sdkName = data[1] as string;
		var index = vrSDKList.serializedProperty.arraySize;
		vrSDKList.serializedProperty.arraySize++;
		vrSDKList.index = index;
		var element = vrSDKList.serializedProperty.GetArrayElementAtIndex(index);
		element.stringValue = sdkName;
		vrSDKList.serializedProperty.serializedObject.ApplyModifiedProperties();
	}
}

}