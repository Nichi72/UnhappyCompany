   using UnityEngine;

   namespace MyUtility
   {
       public static class UtilityGizmos
       {
           /// <summary>
           /// Gizmos를 사용하여 원을 그리는 메서드
           /// </summary>
           /// <param name="center">원 중심</param>
           /// <param name="radius">원 반경</param>
           /// <param name="color">원 색상</param>
           public static void DrawCircle(Vector3 center, float radius, Color color)
           {
               Gizmos.color = color;
               int segments = 36;
               float angle = 360f / segments;
               Vector3 prevPoint = center + new Vector3(radius, 0, 0);
               for (int i = 1; i <= segments; i++)
               {
                   float currentAngle = i * angle * Mathf.Deg2Rad;
                   Vector3 currentPoint = center + new Vector3(radius * Mathf.Cos(currentAngle), 0, radius * Mathf.Sin(currentAngle));
                   Gizmos.DrawLine(prevPoint, currentPoint);
                   prevPoint = currentPoint;
               }
           }
       }
   }