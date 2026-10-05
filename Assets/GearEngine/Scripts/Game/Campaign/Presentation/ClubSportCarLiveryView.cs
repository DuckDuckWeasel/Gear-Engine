using System;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ClubSportCarLiveryView : MonoBehaviour
    {
        private const string k_bodyName = "Body";
        private const string k_roofStripeName = "ClubSportRoofStripe";

        [SerializeField] private Texture2D[] bodyTextures = Array.Empty<Texture2D>();

        public void Apply(int liveryIndex, bool isPlayer)
        {
            if (liveryIndex < 0 || liveryIndex >= bodyTextures.Length || bodyTextures[liveryIndex] == null)
            {
                Debug.LogError($"[ClubSportCarLivery] Missing body texture for livery {liveryIndex}.");
                return;
            }

            MeshRenderer body = Array.Find(
                GetComponentsInChildren<MeshRenderer>(true),
                renderer => renderer.gameObject.name == k_bodyName);
            if (body == null || body.sharedMaterials.Length == 0)
            {
                Debug.LogError("[ClubSportCarLivery] The car requires a Body renderer with at least one material.");
                return;
            }

            ApplyBodyTexture(body, bodyTextures[liveryIndex]);
            AddRoofStripe(body, liveryIndex, isPlayer);
        }

        private static void ApplyBodyTexture(MeshRenderer body, Texture2D texture)
        {
            Material material = body.sharedMaterials[0];
            if (material == null)
            {
                Debug.LogError("[ClubSportCarLivery] The Body paint material is missing.");
                return;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            body.GetPropertyBlock(block, 0);
            SetTexture(block, material, "_BaseMap", texture);
            SetTexture(block, material, "_MainTex", texture);
            SetColor(block, material, "_BaseColor", Color.white);
            SetColor(block, material, "_Color", Color.white);
            SetFloat(block, material, "_Smoothness", 0.22f);
            SetFloat(block, material, "_Metallic", 0.05f);
            body.SetPropertyBlock(block, 0);
        }

        private static void AddRoofStripe(MeshRenderer body, int liveryIndex, bool isPlayer)
        {
            Transform existing = body.transform.Find(k_roofStripeName);
            if (existing != null)
            {
                return;
            }

            MeshFilter filter = body.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                Debug.LogError("[ClubSportCarLivery] The Body mesh is required for the roof stripe.");
                return;
            }

            Bounds bounds = filter.sharedMesh.bounds;
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = k_roofStripeName;
            Collider collider = stripe.GetComponent<Collider>();
            collider.enabled = false;
            DestroyRuntimeObject(collider);
            stripe.transform.SetParent(body.transform, false);
            stripe.transform.localPosition = new Vector3(bounds.center.x, bounds.max.y + 0.018f, bounds.center.z);
            stripe.transform.localScale = new Vector3(
                bounds.size.x * (isPlayer ? 0.24f : 0.11f),
                0.025f,
                bounds.size.z * 0.34f);

            MeshRenderer stripeRenderer = stripe.GetComponent<MeshRenderer>();
            stripeRenderer.sharedMaterial = body.sharedMaterials[Mathf.Min(1, body.sharedMaterials.Length - 1)];
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetTexture("_BaseMap", Texture2D.whiteTexture);
            block.SetTexture("_MainTex", Texture2D.whiteTexture);
            Color color = StripeColor(liveryIndex, isPlayer);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            block.SetFloat("_Smoothness", 0.18f);
            block.SetFloat("_Metallic", 0.02f);
            stripeRenderer.SetPropertyBlock(block);
        }

        private static Color StripeColor(int liveryIndex, bool isPlayer)
        {
            if (isPlayer)
            {
                return new Color32(255, 210, 64, 255);
            }

            return liveryIndex == 1
                ? new Color32(52, 68, 56, 255)
                : new Color32(248, 239, 220, 255);
        }

        private static void SetTexture(MaterialPropertyBlock block, Material material, string property, Texture texture)
        {
            if (material.HasProperty(property))
            {
                block.SetTexture(property, texture);
            }
        }

        private static void SetColor(MaterialPropertyBlock block, Material material, string property, Color color)
        {
            if (material.HasProperty(property))
            {
                block.SetColor(property, color);
            }
        }

        private static void SetFloat(MaterialPropertyBlock block, Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                block.SetFloat(property, value);
            }
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
