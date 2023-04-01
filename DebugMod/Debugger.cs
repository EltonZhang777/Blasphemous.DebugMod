﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework.Managers;
using Gameplay.UI.Others.UIGameLogic;
using ModdingAPI;

namespace DebugMod
{
    public class Debugger : Mod
    {
        public Debugger(string modId, string modName, string modVersion) : base(modId, modName, modVersion) { }
        private const float SCALE_AMOUNT = 0.05f;
        private const string GEOMETRY_NAME = "GEO_Block";
        private const int NUM_TEXT_LINES = 4;
        private const float CAMERA_SPEED = 0.1f;
        private const float CAMERA_MULTIPLIER = 3f;

        private Sprite hitboxImage;
        private List<GameObject> sceneHitboxes = new List<GameObject>();
        private List<Text> textObjects = new List<Text>();
        private Vector3 cameraPosition;

        protected override void Initialize()
        {
            hitboxImage = FileUtil.loadDataImages("hitbox.png", 1, 1, 1, 0, true, out Sprite[] images) ? images[0] : null;
            DisableFileLogging = true;
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                EnabledText = !EnabledText;
            }
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                EnabledHitboxes = !EnabledHitboxes;
            }
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                EnabledFreeCam = !EnabledFreeCam;
            }
        }

        protected override void LateUpdate()
        {
            UpdateDebugText();
            UpdateFreeCam();
        }

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            if (EnabledText)
                ShowDebugText();
            if (EnabledHitboxes)
                ShowHitboxes();
        }

        protected override void LevelUnloaded(string oldLevel, string newLevel)
        {
            HideDebugText();
            HideHitboxes();
            EnabledFreeCam = false;
        }

        #region Debug Text

        private bool _enabledText = false;
        public bool EnabledText
        {
            get { return _enabledText; }
            set
            {
                _enabledText = value;
                if (value)
                {
                    ShowDebugText();
                }
                else
                {
                    HideDebugText();
                }
            }
        }

        private void ShowDebugText()
        {
            if (textObjects.Count == 0)
            {
                CreateDebugText();
            }

            for (int i = 0; i < textObjects.Count; i++)
            {
                textObjects[i].gameObject.SetActive(true);
            }
        }

        private void HideDebugText()
        {
            for (int i = 0; i < textObjects.Count; i++)
            {
                textObjects[i].gameObject.SetActive(false);
            }
        }

        private void UpdateDebugText()
        {
            if (EnabledText && textObjects.Count >= NUM_TEXT_LINES && Core.Logic.Penitent != null)
            {
                textObjects[0].text = $"Scene: " + Core.LevelManager.currentLevel.LevelName;

                Vector2 position = Core.Logic.Penitent.transform.position;
                textObjects[1].text = $"Position: ({RoundToOne(position.x)}, {RoundToOne(position.y)})";

                Vector2 health = new Vector2(Core.Logic.Penitent.Stats.Life.Current, Core.Logic.Penitent.Stats.Life.CurrentMax);
                textObjects[2].text = $"HP: {RoundToOne(health.x)}/{RoundToOne(health.y)}";

                Vector2 fervour = new Vector2(Core.Logic.Penitent.Stats.Fervour.Current, Core.Logic.Penitent.Stats.Fervour.CurrentMax);
                textObjects[3].text = $"FP: {RoundToOne(fervour.x)}/{RoundToOne(fervour.y)}";
            }

            string RoundToOne(float value)
            {
                return value.ToString("F1");
            }
        }

        private void CreateDebugText()
        {
            GameObject textObject = null; Transform parent = null;
            textObjects.Clear();

            foreach (PlayerPurgePoints obj in Object.FindObjectsOfType<PlayerPurgePoints>())
            {
                if (obj.name == "PurgePoints") { textObject = obj.transform.GetChild(1).gameObject; break; }
            }
            foreach (PlayerFervour obj in Object.FindObjectsOfType<PlayerFervour>())
            {
                if (obj.name == "Fervour Bar") { parent = obj.transform; break; }
            }
            if (textObject == null || parent == null) return;

            for (int i = 0; i < NUM_TEXT_LINES; i++)
            {
                GameObject newText = Object.Instantiate(textObject, parent);
                newText.name = "DebugText";
                newText.SetActive(false);

                RectTransform rect = newText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(40f, -45 - (i * 18));
                rect.sizeDelta = new Vector2(250f, 18f);

                Text text = newText.GetComponent<Text>();
                text.color = Color.white;
                text.text = string.Empty;
                text.alignment = TextAnchor.MiddleLeft;

                textObjects.Add(text);
            }
        }

        #endregion Debug Text

        #region Hitboxes

        private bool _enabledHitboxes = false;
        public bool EnabledHitboxes
        {
            get { return _enabledHitboxes; }
            set
            {
                _enabledHitboxes = value;
                if (value)
                {
                    ShowHitboxes();
                }
                else
                {
                    HideHitboxes();
                }
            }
        }

        private void ShowHitboxes()
        {
            GameObject baseHitbox = CreateBaseHitbox();
            sceneHitboxes.Clear();

            foreach (BoxCollider2D collider in Object.FindObjectsOfType<BoxCollider2D>())
            {
                if (collider.name.StartsWith(GEOMETRY_NAME)) continue;

                GameObject hitbox = Object.Instantiate(baseHitbox, collider.transform);
                hitbox.transform.localPosition = Vector3.zero;

                Transform side = hitbox.transform.GetChild(0);
                side.localPosition = new Vector3(collider.offset.x, collider.size.y / 2 + collider.offset.y, 0);
                side.localScale = new Vector3(collider.size.x, SCALE_AMOUNT / collider.transform.localScale.y, 0);

                side = hitbox.transform.GetChild(1);
                side.localPosition = new Vector3(-collider.size.x / 2 + collider.offset.x, collider.offset.y, 0);
                side.localScale = new Vector3(SCALE_AMOUNT / collider.transform.localScale.x, collider.size.y, 0);

                side = hitbox.transform.GetChild(2);
                side.localPosition = new Vector3(collider.size.x / 2 + collider.offset.x, collider.offset.y, 0);
                side.localScale = new Vector3(SCALE_AMOUNT / collider.transform.localScale.x, collider.size.y, 0);

                side = hitbox.transform.GetChild(3);
                side.localPosition = new Vector3(collider.offset.x, -collider.size.y / 2 + collider.offset.y, 0);
                side.localScale = new Vector3(collider.size.x, SCALE_AMOUNT / collider.transform.localScale.y, 0);

                sceneHitboxes.Add(hitbox);
            }

            Object.Destroy(baseHitbox);
            Log($"Adding outlines to {sceneHitboxes.Count} hitboxes");
        }

        private void HideHitboxes()
        {
            for (int i = 0; i < sceneHitboxes.Count; i++)
            {
                if (sceneHitboxes[i] != null)
                    Object.Destroy(sceneHitboxes[i]);
            }
            sceneHitboxes.Clear();
        }

        private GameObject CreateBaseHitbox()
        {
            GameObject baseHitbox = new GameObject("Hitbox");
            GameObject side = new GameObject("TOP");
            side.AddComponent<SpriteRenderer>().sprite = hitboxImage;
            side.transform.parent = baseHitbox.transform;
            side = new GameObject("LEFT");
            side.AddComponent<SpriteRenderer>().sprite = hitboxImage;
            side.transform.parent = baseHitbox.transform;
            side = new GameObject("RIGHT");
            side.AddComponent<SpriteRenderer>().sprite = hitboxImage;
            side.transform.parent = baseHitbox.transform;
            side = new GameObject("BOTTOM");
            side.AddComponent<SpriteRenderer>().sprite = hitboxImage;
            side.transform.parent = baseHitbox.transform;
            return baseHitbox;
        }

        #endregion Hitboxes

        #region Free Cam

        private bool _enabledFreeCam = false;
        public bool EnabledFreeCam
        {
            get { return _enabledFreeCam; }
            set { _enabledFreeCam = value; }
        }

        private void UpdateFreeCam()
        {
            if (Camera.main == null) return;

            if (EnabledFreeCam)
            {
                float camSpeed = Input.GetKey(KeyCode.LeftControl) ? CAMERA_SPEED * CAMERA_MULTIPLIER : CAMERA_SPEED;

                if (Input.GetKey(KeyCode.LeftArrow))
                    cameraPosition += Vector3.left * camSpeed;
                if (Input.GetKey(KeyCode.RightArrow))
                    cameraPosition += Vector3.right * camSpeed;
                if (Input.GetKey(KeyCode.DownArrow))
                    cameraPosition += Vector3.down * camSpeed;
                if (Input.GetKey(KeyCode.UpArrow))
                    cameraPosition += Vector3.up * camSpeed;
                Camera.main.transform.position = cameraPosition;
            }
            else
            {
                cameraPosition = Camera.main.transform.position;
            }
        }

        #endregion Free Cam
    }
}
