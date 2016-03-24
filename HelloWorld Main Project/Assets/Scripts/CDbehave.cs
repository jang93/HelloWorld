using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CDbehave : MonoBehaviour {

    public Skill skill;


    // Update is called once per frame
    void Update()
    {
        
            if (this.skill.currentCoolDown < this.skill.cooldown)
            {
            this.skill.cooldown = this.skill.cooldown + Time.deltaTime;
            this.skill.skillIcon.fillAmount = this.skill.currentCoolDown / this.skill.cooldown;
            }
        
    }
}


