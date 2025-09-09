using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

public static class BuildMainMenuPrefab
{
    [MenuItem("Tools/Tetris/Create Main Menu Prefab")]
    public static void CreateMainMenuPrefab()
    {
        // ---- 顏色表 ----
        Color colBtnNormal = FromHex("#FFF3E8FF");   // 按鈕底色
        Color colBtnHi     = FromHex("#FFD755FF");   // Highlight
        Color colBtnPress  = FromHex("#F6A5C6FF");   // Pressed
        Color colText      = FromHex("#1C1C3AFF");   // 文字
        Color colShade     = new Color(0,0,0,0.45f); // 頂部遮罩

        // ---- 臨時場景物件容器 ----
        var root = new GameObject("MainMenuUI");

        // EventSystem
        var es = new GameObject("EventSystem");
        es.transform.SetParent(root.transform, false);
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(root.transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // 背景 BG
        var bg = NewImage("BG", canvasGO.transform);
        StretchFull(bg.rectTransform);
        var bgSprite = FindSpriteByNameContains("background");
        if (bgSprite) bg.sprite = bgSprite;
        bg.preserveAspect = false; // 填滿

        // 頂部半透明遮罩 TopShade
        var shade = NewImage("TopShade", canvasGO.transform);
        var rtShade = shade.rectTransform;
        rtShade.anchorMin = new Vector2(0f, 1f);
        rtShade.anchorMax = new Vector2(1f, 1f);
        rtShade.pivot     = new Vector2(0.5f, 1f);
        rtShade.sizeDelta = new Vector2(0f, 420f);
        rtShade.anchoredPosition = new Vector2(0f, 0f);
        shade.raycastTarget = false;
        shade.color = colShade;

        // Logo
        var logoImg = NewImage("Logo", canvasGO.transform);
        var rtLogo = logoImg.rectTransform;
        rtLogo.anchorMin = rtLogo.anchorMax = new Vector2(0.5f, 1f);
        rtLogo.pivot = new Vector2(0.5f, 1f);
        rtLogo.anchoredPosition = new Vector2(0f, -150f);
        rtLogo.sizeDelta = new Vector2(900f, 450f);
        logoImg.preserveAspect = true;
        var logoSprite = FindSpriteByNameContains("tetris") ?? FindSpriteByNameContains("logo");
        if (logoSprite) logoImg.sprite = logoSprite;

        // Buttons 容器
        var btnRoot = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        btnRoot.transform.SetParent(canvasGO.transform, false);
        var rtBtns = btnRoot.GetComponent<RectTransform>();
        rtBtns.anchorMin = rtBtns.anchorMax = new Vector2(0.5f, 0.5f);
        rtBtns.pivot = new Vector2(0.5f, 0.5f);
        rtBtns.anchoredPosition = new Vector2(0f, -280f);

        var vlg = btnRoot.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 20f;

        var csf = btnRoot.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 建立三顆按鈕
        var singleBtn = CreateButton(btnRoot.transform, "SinglePlayer", "Single Player", 500, 110, colBtnNormal, colText);
        var aiBtn     = CreateButton(btnRoot.transform, "PlayWithAI", "Play With AI", 500, 110, colBtnNormal, colText);
        var multiBtn  = CreateButton(btnRoot.transform, "Multiplayer", "Multiplayer", 500, 110, colBtnNormal, colText);

        // 統一 Button ColorTint
        SetupButtonTint(singleBtn, colBtnNormal, colBtnHi, colBtnPress);
        SetupButtonTint(aiBtn,     colBtnNormal, colBtnHi, colBtnPress);
        SetupButtonTint(multiBtn,  colBtnNormal, colBtnHi, colBtnPress);

        // 陰影（質感）
        AddShadow(singleBtn.gameObject, 0.25f, new Vector2(0, -4));
        AddShadow(aiBtn.gameObject,     0.25f, new Vector2(0, -4));
        AddShadow(multiBtn.gameObject,  0.25f, new Vector2(0, -4));

        // 控制器（掛事件）
        var controller = new GameObject("MainMenuController", typeof(MainMenuController));
        controller.transform.SetParent(root.transform, false);

        BindOnClick(singleBtn, controller, nameof(MainMenuController.PlaySinglePlayer));
        BindOnClick(aiBtn,     controller, nameof(MainMenuController.PlayWithAI));
        BindOnClick(multiBtn,  controller, nameof(MainMenuController.PlayMultiplayer));

        // 建資料夾 & 存 Prefab
        var uiFolder = "Assets/UI";
        if (!AssetDatabase.IsValidFolder(uiFolder))
        {
            AssetDatabase.CreateFolder("Assets", "UI");
        }
        var prefabPath = uiFolder + "/MainMenu.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

        // 清理臨時物件
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Prefab 建立完成：{prefabPath}\n把它拖進場景即可使用。", "OK");
    }

    // ---------- 工具方法 ----------
    private static Image NewImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(Image));
        go.transform.SetParent(parent, false);
        return go.GetComponent<Image>();
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Button CreateButton(Transform parent, string goName, string label, float w, float h, Color bg, Color txt)
    {
        var go = new GameObject(goName, typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);

        var img = go.GetComponent<Image>();
        img.color = bg;

        // 文字
#if TMP_PRESENT || UNITY_TEXTMESHPRO
        var tgo = new GameObject("Text", typeof(TextMeshProUGUI));
        tgo.transform.SetParent(go.transform, false);
        var tr = tgo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

        var tmp = tgo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 44;
        tmp.color = txt;
        var outline = tgo.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1.2f, -1.2f);
#else
        var tgo = new GameObject("Text", typeof(Text));
        tgo.transform.SetParent(go.transform, false);
        var tr = tgo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

        var utxt = tgo.GetComponent<Text>();
        utxt.text = label;
        utxt.alignment = TextAnchor.MiddleCenter;
        utxt.fontSize = 28;
        utxt.color = txt;
#endif
        return go.GetComponent<Button>();
    }

    private static void SetupButtonTint(Button btn, Color normal, Color hi, Color press)
    {
        var cb = btn.colors;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        cb.normalColor = normal;
        cb.highlightedColor = hi;
        cb.pressedColor = press;
        cb.selectedColor = hi;
        cb.disabledColor = new Color(1f,1f,1f,0.4f);
        btn.colors = cb;
    }

    private static void AddShadow(GameObject go, float alpha, Vector2 distance)
    {
        var s = go.AddComponent<Shadow>();
        s.effectColor = new Color(0,0,0,alpha);
        s.effectDistance = distance;
    }

    private static void BindOnClick(Button btn, GameObject target, string method)
    {
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        int index = calls.arraySize;
        calls.InsertArrayElementAtIndex(index);
        var call = calls.GetArrayElementAtIndex(index);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target.GetComponent<MainMenuController>();
        call.FindPropertyRelative("m_MethodName").stringValue = method;
        call.FindPropertyRelative("m_Mode").intValue = 1; // PersistentListenerMode.Void
        call.FindPropertyRelative("m_CallState").intValue = 2; // RuntimeOnly
        so.ApplyModifiedProperties();
    }

    private static Sprite FindSpriteByNameContains(string keywordLower)
    {
        keywordLower = keywordLower.ToLower();
        foreach (var guid in AssetDatabase.FindAssets("t:Sprite"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null)
            {
                var name = Path.GetFileNameWithoutExtension(path).ToLower();
                if (name.Contains(keywordLower)) return sp;
            }
        }
        return null;
    }

    private static Color FromHex(string hexRGBA)
    {
        Color c;
        if (ColorUtility.TryParseHtmlString(hexRGBA, out c)) return c;
        return Color.white;
    }
}
#endif
