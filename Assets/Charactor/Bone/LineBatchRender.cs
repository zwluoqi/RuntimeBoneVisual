using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Content.Scritps.Charactor
{
    

    public class LineBatchRender
    {
        private List<Vector3> starts = new List<Vector3>();
        private List<Vector3> ends = new List<Vector3>();
        private List<Color> colors = new List<Color>();

        public void Clear()
        {
            starts.Clear();
            ends.Clear();
            colors.Clear();
        }

        public void AddInstance(Vector3 start,Vector3 end,Color col)
        {
            starts.Add(start);
            ends.Add(end);
            colors.Add(col);
        }

        public void Render()
        {
            for (int i = 0; i < starts.Count; i++)
            {
                Gizmos.Line(starts[i], ends[i],colors[i]);    
            }
            
        }
        
    }

}