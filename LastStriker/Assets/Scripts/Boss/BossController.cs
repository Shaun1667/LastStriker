
using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public BossPartHealth leftArm;
    public BossPartHealth rightArm;
    public BossPartHealth backPack;
    public BossPartHealth core;

    public BossArmAttack leftArmAttack;
    public BossArmAttack rightArmAttack;
    public BossSummoner backPackSummoner;

    public Renderer coreRenderer;
    [Header("Core Models (optional)")]
    [Tooltip("Shown while the core is protected. When both models are assigned they replace the colour tint.")]
    public GameObject coreProtectedModel;
    [Tooltip("Shown while the core is exposed and can be damaged.")]
    public GameObject coreExposedModel;
    public Color coreProtectedColor = new Color(0.2f, 0.2f, 0.2f);
    public Color coreExposedColor = new Color(1f, 0.1f, 0.9f);

    public float coreExposureDuration = 6f;
    public int coreHeavyDamageMin = 60;
    public int coreHeavyDamageMax = 80;
    public float heavyAttackDelay = 3f;

    public GameManager gameManager;
    public System.Action OnBossDefeated;

    bool armsDownHandled;
    bool defeated;

    public void Activate()
    {
        gameObject.SetActive(true);
        core.damageBlocked = true;
        SetCoreExposed(false);
        if (backPack != null) backPack.gameObject.SetActive(false);

        leftArm.OnDepleted += HandleArmDepleted;
        rightArm.OnDepleted += HandleArmDepleted;
        core.OnDepleted += HandleCoreDepleted;

        if (gameManager != null) gameManager.SetStatusMessage("BOSS APPEARED");
    }

    void SetCoreExposed(bool exposed)
    {
        if (coreProtectedModel != null || coreExposedModel != null)
        {
            if (coreProtectedModel != null) coreProtectedModel.SetActive(!exposed);
            if (coreExposedModel != null) coreExposedModel.SetActive(exposed);
            return;
        }
        if (coreRenderer != null) coreRenderer.material.color = exposed ? coreExposedColor : coreProtectedColor;
    }

    void HandleArmDepleted(BossPartHealth part)
    {
        if (armsDownHandled || defeated) return;
        if (leftArm.CurrentHP <= 0 && rightArm.CurrentHP <= 0)
        {
            armsDownHandled = true;
            StartCoroutine(CoreExposureWindow());
        }
    }

    IEnumerator CoreExposureWindow()
    {
        if (gameManager != null) gameManager.SetStatusMessage("CORE EXPOSED!");
        core.damageBlocked = false;
        SetCoreExposed(true);

        float elapsed = 0f;
        float heavyTimer = heavyAttackDelay;
        bool heavyFired = false;
        while (elapsed < coreExposureDuration && core.CurrentHP > 0 && !defeated)
        {
            elapsed += Time.deltaTime;
            heavyTimer -= Time.deltaTime;
            if (!heavyFired && heavyTimer <= 0f)
            {
                heavyFired = true;
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                {
                    PlayerHealth ph = p.GetComponent<PlayerHealth>();
                    if (ph != null) ph.TakeDamage(Random.Range(coreHeavyDamageMin, coreHeavyDamageMax + 1), true);
                }
            }
            yield return null;
        }

        if (defeated) yield break;

        if (core.CurrentHP > 0)
        {
            core.damageBlocked = true;
            SetCoreExposed(false);
            StartCoroutine(EnterNextCycle());
        }
    }

    IEnumerator EnterNextCycle()
    {
        if (gameManager != null) gameManager.SetStatusMessage("BOSS REGROUPING");
        yield return new WaitForSeconds(1.5f);

        leftArm.ResetPart();
        rightArm.ResetPart();
        armsDownHandled = false;

        if (backPack != null && !backPack.gameObject.activeSelf)
        {
            backPack.gameObject.SetActive(true);
            backPack.ResetPart();
        }

        if (gameManager != null) gameManager.SetStatusMessage("BOSS PHASE 2");
    }

    void HandleCoreDepleted(BossPartHealth part)
    {
        defeated = true;
        StopAllCoroutines();
        if (leftArmAttack != null) leftArmAttack.enabled = false;
        if (rightArmAttack != null) rightArmAttack.enabled = false;
        if (backPackSummoner != null) backPackSummoner.enabled = false;
        if (gameManager != null) gameManager.SetStatusMessage("BOSS DEFEATED!");
        OnBossDefeated?.Invoke();
    }
}
