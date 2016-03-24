using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SkillCD : MonoBehaviour {

    public List<Skill> skills;
    void Start()
    {
        foreach (var skill in skills)
        {
            skill.currentCoolDown = skill.cooldown;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (skills[0].currentCoolDown>= skills[0].cooldown)
            {
                //do something
                skills[0].currentCoolDown = 0;

            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (skills[1].currentCoolDown >= skills[1].cooldown)
            {
                //do something
                skills[1].currentCoolDown = 0;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (skills[2].currentCoolDown >= skills[2].cooldown)
            {
                //do something
                skills[2].currentCoolDown = 0;
            }
        }
    }
    void Update()
    {
        foreach(Skill s in skills)
        {
            if (s.currentCoolDown < s.cooldown)
            {
                s.currentCoolDown +=  Time.deltaTime;
                s.skillIcon.fillAmount = s.currentCoolDown / s.cooldown;
            }
        }
    }



}

[System.Serializable]
public class Skill
{
    public float cooldown;
    public Image skillIcon;
    [HideInInspector]
    public float currentCoolDown;
}
