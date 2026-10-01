using UnityEngine;
namespace Herbalist.Levels
{
    public enum ExteriorBranchKind { Normal, JumpPad }
    [System.Serializable]
    public sealed class ExteriorBranchLayout
    {
        public string name;
        public ExteriorBranchKind kind;
        public float angle, height;
        public float innerRadius = 3, length = 7, width = 2.6f;
    }
    [System.Serializable]
    public sealed class ExteriorClothLayout
    {
        public string name;
        public float angle, anchorHeight = 39, radius = 10, length;
    }
    [System.Serializable]
    public sealed class ExteriorLeafBridgeLayout
    {
        public string name;
        public float fromAngle, toAngle, fromHeight, toHeight;
        public float radius = 3, width = 2.4f, thickness = .3f;
        public int wallSections = 3;
    }
    [CreateAssetMenu(menuName = "Herbalist/Stage/Exterior PDF Layout")]
    public sealed class ExteriorLayoutSettings : ScriptableObject
    {
        public ExteriorBranchLayout[] branches;
        public ExteriorClothLayout[] cloths;
        public ExteriorLeafBridgeLayout[] leafBridges;
        public Vector3[] spawnOffsets;
        public Vector3[] herbOffsets;
        public float summitHeight = 41;
        public float clothDriveAcceleration = 120;
        public Vector3 summitDeckSize = new Vector3(6, .6f, 6);
        public Material jumpBranchMaterial, normalBranchMaterial, clothMaterial, leafBridgeMaterial, portMaterial;
        public Material jetMaterial;
        [Header("Water droplets around the jump pad jet")]
        [Min(0)] public float jetDropletRate = 42f;
        public Vector2 jetDropletLifetime = new Vector2(.45f, .85f);
        public Vector2 jetDropletSpeed = new Vector2(.8f, 2.4f);
        public Vector2 jetDropletSize = new Vector2(.045f, .13f);
        [Min(0)] public float jetDropletGravity = 1.6f;
        public SapJumpPadSettings jumpPadSettings;
        public string padInstructions = "나뭇잎 위 탑승 · 반대 구멍에 R로 수액 · 자동 상승 후 WASD 착지";
        public string clothInstructions = "E: 매듭 잡기/놓기 · WASD: 흔들기";
        public string bridgeInstructions = "나뭇잎 다리 구간 · 벽에 R로 나뭇잎을 설치해 올라가세요";
    }
}
