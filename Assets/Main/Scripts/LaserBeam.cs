using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam
{
    private Vector3 pos, dir;
    private GameObject laserObj;
    private LineRenderer laser;
    private List<Vector3> laserIndices = new List<Vector3>();

    public LaserBeam(Vector3 pos, Vector3 dir, Material material)
    {
        this.laser = new LineRenderer();
        this.laserObj = new GameObject();
        this.laserObj.name = "Laser Beam";
        this.pos = pos;
        this.dir = dir;
        
        this.laser = this.laserObj.AddComponent(typeof(LineRenderer)) as LineRenderer;
        this.laser.startWidth = 0.3f;
        this.laser.endWidth = 0.3f;
        this.laser.textureMode = LineTextureMode.Tile;
        this.laser.material = material;

        CastRay(pos, dir, laser);
    }

    private void CastRay(Vector3 pos, Vector3 dir, LineRenderer laser)
    {
        laserIndices.Add(new Vector3(pos.x, 0.08f, pos.z));

        Ray ray = new Ray(pos, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 30, 1))
        {
            laserIndices.Add(new Vector3(hit.point.x, 0.08f, hit.point.z));
            UpdateLaser();
        }
        else
        {
            Vector3 hitPos = ray.GetPoint(30);
            laserIndices.Add(new Vector3(hitPos.x, 0.08f, hitPos.z));
            UpdateLaser();
        }
            
    }

    private void UpdateLaser()
    {
        int count = 0;
        laser.positionCount = laserIndices.Count;

        foreach (Vector3 idx in laserIndices)
        {
            laser.SetPosition(count, idx);
            count++;
        }
    }

    private void CheckHit(RaycastHit hitInfo, Vector3 direction, LineRenderer laser)
    {
        if (hitInfo.collider.CompareTag("MirrorWall") && laserIndices.Count < 3)
        {
            Vector3 pos = hitInfo.point;
            Vector3 dir = Vector3.Reflect(direction, hitInfo.normal);
            
            CastRay(pos, dir, laser);
        }
        else
        {
            laserIndices.Add(hitInfo.point);
            UpdateLaser();
        }
    }
}
