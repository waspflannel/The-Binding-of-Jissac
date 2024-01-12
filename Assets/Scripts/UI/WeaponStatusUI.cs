using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class WeaponStatusUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("Object References")]
    #endregion Header OBJECT REFERENCES
    [SerializeField] private Image weaponImage;
    [SerializeField] private Transform ammoHolderTransform;//parent object to hold all ammo in clip
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private TextMeshProUGUI ammoRemainingText;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Transform reloadBar;
    [SerializeField] private Image barImage;

    private Player player;
    private List<GameObject> ammoIconList = new List<GameObject>();
    private Coroutine reloadWeaponCoroutine;
    private Coroutine blinkingReloadTextCoroutine;


    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        //subscribe to setActiveWeaponEvent
        player.setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;
        //subscribe to weaponFiredEvent
        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;
        //subscribe to reloadWeaponEvent
        player.reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnWeaponReload;
        //subscribe to weaponReloadedEvent
        player.weaponReloadedEvent.OnWeaponReloaded += WeaponReloadedEvent_OnWeaponReloaded;
    }
    private void OnDisable()
    {
        //unsubscribe to setActiveWeaponEvent
        player.setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;
        //unsubscribe to weaponFiredEvent
        player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;
        //unsubscribe to reloadWeaponEvent
        player.reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnWeaponReload;
        //unsubscribe to weaponReloadedEvent
        player.weaponReloadedEvent.OnWeaponReloaded -= WeaponReloadedEvent_OnWeaponReloaded;
    }

    private void Start()
    {
        //at the start set to start weapon
        SetActiveWeapon(player.activeWeapon.GetCurrentWeapon());
    }

    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        //on weapon swap , change the weapon
        SetActiveWeapon(setActiveWeaponEventArgs.weapon);
    }
    private void SetActiveWeapon(Weapon weapon)
    {
        UpdateActiveWeaponImage(weapon.weaponDetails);//update image
        UpdateActiveWeaponName(weapon);//update name
        UpdateAmmotext(weapon);//update ammo text
        UpdateAmmoLoadedIcons(weapon);//update ammo clip icons

        if (weapon.isWeaponReloading)//if we switch and weapon is still reloaded
        {
            UpdateWeaponReloadBar(weapon);//update reload bar
        }
        else
        {
            ResetWeaponReloadBar();//reset the reload bar
        }
        UpdateReloadText(weapon);//update the reload text
    }

    private void WeaponFiredEvent_OnWeaponFired(WeaponFiredEvent weaponFiredEvent, WeaponFiredEventArgs weaponFiredEventArgs)
    {
        WeaponFired(weaponFiredEventArgs.weapon);
    }
    private void WeaponFired(Weapon weapon)
    {
   
        UpdateAmmotext(weapon);
        UpdateAmmoLoadedIcons(weapon);
        UpdateReloadText(weapon);
    }
    private void ReloadWeaponEvent_OnWeaponReload(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        UpdateWeaponReloadBar(reloadWeaponEventArgs.weapon);
    }

    private void WeaponReloadedEvent_OnWeaponReloaded(WeaponReloadedEvent weaponReloadedEvent, WeaponReloadedEventArgs weaponReloadedEventArgs)
    {
        WeaponReloaded(weaponReloadedEventArgs.weapon);
    }
    private void WeaponReloaded(Weapon weapon)
    {
        if (player.activeWeapon.GetCurrentWeapon() == weapon)
        {
            UpdateReloadText(weapon);
            UpdateAmmotext(weapon);
            UpdateAmmoLoadedIcons(weapon);
            ResetWeaponReloadBar();
        }
    }

    private void UpdateActiveWeaponImage(WeaponDetailsSO weaponDetails)
    {
        weaponImage.sprite = weaponDetails.weaponSprite;
    }

    private void UpdateActiveWeaponName(Weapon weapon)
    {
        //recall weaponListPosition is the index where its in the starting weapon list (Player Class)
        weaponNameText.text = "(" + weapon.weaponListPosition + ")" + weapon.weaponDetails.weaponName.ToUpper();

    }

    private void UpdateAmmotext(Weapon weapon)
    {
        if (weapon.weaponDetails.hasInfiniteAmmo)
        {
            ammoRemainingText.text = "INFINITE AMMO";
        }
        else
        {
            //weaponRemainingAmmo is altered in fireWeapon class
            //ammoRemainingText.text = weapon.weaponRemainingAmmo.ToString()+ "/" + weapon.weaponDetails.weaponAmmoCapacity.ToString();
            ammoRemainingText.text = weapon.weaponClipRemainingAmmo + "/" + weapon.weaponRemainingAmmo.ToString();
        }
    }

    private void UpdateAmmoLoadedIcons(Weapon weapon)
    {
        ClearAmmoLoadedIcons();

        for(int i=0; i < weapon.weaponClipRemainingAmmo; i++)
        {
            //instantiate the icon, icon is held in gameResources. 
            //goes under ammoHolderTransform since its the parent
            GameObject ammoIcon = Instantiate(GameResources.Instance.ammoIconPrefab, ammoHolderTransform);
            //space each bullet icon
            ammoIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, Settings.uiAmmoIconSpacing * i);
            ammoIconList.Add(ammoIcon);
        }
    }

    private void ClearAmmoLoadedIcons()
    {
        foreach(GameObject ammoIcon in ammoIconList)
        {
            Destroy(ammoIcon);
        }
        ammoIconList.Clear();
    }

    private void UpdateWeaponReloadBar(Weapon weapon)
    {
        if (weapon.weaponDetails.hasInfiniteClipCapacity)
        {
            return;
        }

        StopReloadWeaponCoroutine();
        UpdateReloadText(weapon);

        reloadWeaponCoroutine = StartCoroutine(UpdateWeaponReloadBarRoutine(weapon));
    }

    private IEnumerator UpdateWeaponReloadBarRoutine(Weapon weapon)
    {
        barImage.color = Color.red;

        while (weapon.isWeaponReloading)
        {
            //while the weapon is reloading get a value for how much the bar should be filled (0-1)
            float barFill = weapon.weaponReloadTimer / weapon.weaponDetails.weaponReloadTime;

            //fill in the bar
            reloadBar.transform.localScale = new Vector3(barFill, 1f, 1f);

            yield return null;
        }
    }

    private void ResetWeaponReloadBar()
    {
        StopReloadWeaponCoroutine();

        barImage.color = Color.green;
        reloadBar.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void StopReloadWeaponCoroutine()
    {
        if(reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }
    }

    private void UpdateReloadText(Weapon weapon)
    {
        if( (!weapon.weaponDetails.hasInfiniteClipCapacity) && (weapon.weaponClipRemainingAmmo <=0 || weapon.isWeaponReloading))
        {
            StopBlinkingReloadTextCoroutine();

            blinkingReloadTextCoroutine = StartCoroutine(StartBlinkingReloadTextCoroutine());
        }
        else
        {
            StopBlinkingReloadText();
        }
    }

    private IEnumerator StartBlinkingReloadTextCoroutine()
    {
        while (true)
        {
            reloadText.text = "RELOAD";
            yield return new WaitForSeconds(0.3f);
            reloadText.text = "";
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void StopBlinkingReloadText()
    {
        StopBlinkingReloadTextCoroutine();

        reloadText.text = "";
    }

    private void StopBlinkingReloadTextCoroutine()
    {
        if(blinkingReloadTextCoroutine != null)
        {
            StopCoroutine(blinkingReloadTextCoroutine);
        }
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponImage), weaponImage);
        HelperUtilities.ValidateCheckNullValues(this, nameof(ammoHolderTransform), ammoHolderTransform);
        HelperUtilities.ValidateCheckNullValues(this, nameof(reloadText), reloadText);
        HelperUtilities.ValidateCheckNullValues(this, nameof(ammoRemainingText), ammoRemainingText);
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponNameText), weaponNameText);
        HelperUtilities.ValidateCheckNullValues(this, nameof(reloadBar), reloadBar);
        HelperUtilities.ValidateCheckNullValues(this, nameof(barImage), barImage);
    }

#endif
}


