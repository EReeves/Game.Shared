using System;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace Game.Shared.Components
{
    internal class MapCollider
    {
        public PolygonCollider[] colliders;
        private readonly TiledMap tiledMap;

        public MapCollider(TiledMap map)
        {
            tiledMap = map;
            var collidersGroup = tiledMap.getObjectGroup("Colliders");
            colliders = new PolygonCollider[collidersGroup.objects.Length];

            for (var i = 0; i < collidersGroup.objects.Length; i++)
            {
                var obj = collidersGroup.objects[i];

                /*
                //Rotate around local origin.
                var rads = MathHelper.ToRadians(30);
                var points = new Vector2[4];
                points[0] = new Vector2(0, 0);//Topleft
                points[1] = new Vector2(obj.width,0);//Topright
                points[2] = new Vector2(obj.width, obj.height);//BotRight
                points[3] = new Vector2(0, obj.height);//BotLeft
                
                for (var o = 0; o < 4; o++)
                {
                    points[o] += new Vector2(obj.x, obj.y);
                    points[o] = points[o].rotate(rads);
                }    */

                var points = new Vector2[4];
                points[0] = new Vector2(obj.x, obj.y); //Topleft
                points[1] = new Vector2(obj.x + obj.width, obj.y); //Topright
                points[2] = new Vector2(obj.x + obj.width, obj.y + obj.height); //BotRight
                points[3] = new Vector2(obj.x, obj.y + obj.height); //BotLeft

                //var offset = new Vector2(-500, 1500);
                var tileDimensions = new Vector2(68, 78);

                var sqrtthree = (float) Math.Sqrt(3);
                var sqrttwo = (float) Math.Sqrt(2);

                var matrix = new Matrix(
                    new Vector4(sqrtthree, 0, -sqrtthree, 0),
                    new Vector4(1, 2, 1, 0),
                    new Vector4(sqrttwo, -sqrttwo, sqrttwo, 0),
                    new Vector4(0, 0, 0, 1));

                var m = Matrix.Multiply(matrix, (float) (1 / Math.Sqrt(6)));


                for (var o = 0; o < 4; o++)
                {
                    var original = new Vector3(points[o].X, points[o].Y, 0);

                    var vec = Vector3.Transform(original, m);

                    points[o].X = vec.X;
                    points[o].Y = vec.Y;
                }


                colliders[i] = new PolygonCollider(points);
                colliders[i].setShouldColliderScaleAndRotateWithTransform(true);
                //entity.addComponent<BoxCollider>(colliders[i]);
            }

            Vector2 isometricTileToWorldPosition(float x, float y)
            {
                var xp = x / map.tileWidth;
                var yp = y / map.tileHeight;

                var worldX = xp * map.tileWidth / 2 - y * map.tileWidth / 2 +
                             ((float) map.height - 1) * map.tileWidth / 2;
                var worldY = yp * map.tileHeight / 2 + x * map.tileHeight / 2;
                return new Vector2(worldX, worldY);
            }
        }
    }
}