using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace VivaMente
{
    // Device-relative seated origin; no artificial locomotion.
    public sealed class GardenInput : MonoBehaviour
    {
        public Camera View;
        private readonly bool[] held = new bool[2];
        private readonly LineRenderer[] rays = new LineRenderer[2];
        private readonly XRNode[] hands = { XRNode.LeftHand, XRNode.RightHand };
        private void Start()
        {
            for (int i = 0; i < 2; i++)
            {
                rays[i] = new GameObject("Ponteiro " + hands[i]).AddComponent<LineRenderer>();
                rays[i].positionCount = 2;
                rays[i].startWidth = rays[i].endWidth = .008f;
                rays[i].material = new Material(Shader.Find("Sprites/Default"));
                rays[i].startColor = rays[i].endColor = Color.white;
            }
        }
        private void Update()
        {
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 hp))
                View.transform.localPosition = hp;
            if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion hr))
                View.transform.localRotation = hr;
            for (int i = 0; i < 2; i++)
            {
                var hand = InputDevices.GetDeviceAtXRNode(hands[i]);
                bool valid = hand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 p)
                    && hand.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked;
                rays[i].enabled = valid;
                if (!valid) { held[i] = false; continue; }
                hand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);
                var origin = View.transform.parent.TransformPoint(p);
                var direction = View.transform.parent.TransformDirection(rotation * Vector3.forward);
                var ray = new Ray(origin, direction);
                bool hit = Physics.Raycast(ray, out RaycastHit info, 8f);
                rays[i].SetPosition(0, origin);
                rays[i].SetPosition(1, hit ? info.point : origin + direction * 8f);
                hand.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed);
                if (pressed && !held[i] && hit) info.collider.GetComponent<GardenAction>()?.Activate?.Invoke();
                held[i] = pressed;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                var ray = View.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 8f))
                    hit.collider.GetComponent<GardenAction>()?.Activate?.Invoke();
            }
        }
    }
}
