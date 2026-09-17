using UnityEngine;
namespace Herbalist.Levels
{
    public sealed class LevelRouteGuide : MonoBehaviour
    {
        public Transform[] expectedRoute;
        public Color color=Color.cyan;
        public float markerRadius=.4f;
        private void OnDrawGizmos()
        {
            if(expectedRoute==null)return; Gizmos.color=color;
            for(int i=0;i<expectedRoute.Length;i++)
            {
                if(expectedRoute[i]==null)continue; Gizmos.DrawWireSphere(expectedRoute[i].position,markerRadius);
                if(i>0&&expectedRoute[i-1]!=null)Gizmos.DrawLine(expectedRoute[i-1].position,expectedRoute[i].position);
            }
        }
    }
}
