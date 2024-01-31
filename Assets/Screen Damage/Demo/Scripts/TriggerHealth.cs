using UnityEngine;

namespace ScreenDamageSpace
{
    public class TriggerHealth : MonoBehaviour
    {
        private ScreenDamage script;

        void Start()
        {   //g et the main screen damage script
            script = GetComponent<ScreenDamage>();
        }

        // decrease health 
        public void DecreaseHealth(){
            script.CurrentHealth -= 10f;
        }

        // increase health
        public void IncreaseHealth(){
            script.CurrentHealth += 10f;
        }
    }
}