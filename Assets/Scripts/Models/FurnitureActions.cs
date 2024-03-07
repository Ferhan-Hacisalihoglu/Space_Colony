using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public static class FurnitureActions 
    {
        // Door actions

        public static void Door_UpdateActions(Furniture furn,float deltaTime)
        {
            //Debug.Log("Door_FixedUpdateActions: " + furn.GetParameter("openness"));

            if (furn.GetParameter("is_opening") >= 1f)
            {
                furn.SetParameter("openness",furn.GetParameter("openness")+(deltaTime*4)); //Açılış hızına bie değişken ata

                if (furn.GetParameter("openness") >= 1f)
                {
                    furn.SetParameter("is_opening",0);
                }
            }
            else
            {
                furn.SetParameter("openness",furn.GetParameter("openness")-(deltaTime*4));
            }

            furn.SetParameter("openness",Mathf.Clamp01(furn.GetParameter("openness")));

            if (furn.cbOnChanged != null)
            {
                furn.cbOnChanged(furn);
            }
        }

        public static Enterability Door_IsEnterable(Furniture furn)
        {
            furn.SetParameter("is_opening",1);

            if(furn.GetParameter("openness") >= 1)
            {
                return Enterability.Yes; 
            }

            return Enterability.Soon;
        }

    }
}
