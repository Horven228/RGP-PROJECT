using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BossWizardStateMachine : MageStateMachine
{
    [Header("Настройки босса-мага")]
    [SerializeField] private float _weaponChangeHealthThreshold = 0.5f;

    [Header("Оружие (только визуал)")]
    [SerializeField] private GameObject _staffPhase1;
    [SerializeField] private GameObject _staffPhase2;

    [Header("Магические атаки (4 стихии)")]
    [SerializeField] private List<ElementalAttack> _elementalAttacks;

    [Header("Точки спавна")]
    [SerializeField] private Transform _singleSpawnPoint;
    [SerializeField] private Transform[] _doubleSpawnPoints;

    [Header("Настройки атак")]
    [SerializeField] private float _normalAttackCooldown = 2f;
    [SerializeField] private float _strongAttackCooldown = 5f;
    [SerializeField] private float _elementChangeInterval = 8f;

    private ElementType _currentElement;
    private float _lastElementChangeTime;
    private float _lastNormalAttackTime;
    private float _lastStrongAttackTime;
    private bool _isPhase2 = false;           // Фаза босса (false = 1 фаза, true = 2 фаза)
    private bool _isAttacking = false;
    private bool _startedWithPhase2 = false;   // С каким оружием заспавнился (false = Phase1, true = Phase2)

    public ElementType CurrentElement => _currentElement;
    public bool IsPhase2 => _isPhase2;

    protected override void Awake()
    {
        base.Awake();
        AttackRange = 15f;
        ChaseRange = 20f;
    }

    protected override void Start()
    {
        base.Start();

        // Босс всегда начинает с 1 фазы
        _isPhase2 = false;

        //  Выбираем рандомное визуальное оружие при спавне (50% / 50%)
        ChooseRandomWeaponVisual();

        // Выбираем случайную стихию
        ChangeToRandomElement();
        _lastElementChangeTime = Time.time;

        // Обновляем визуал оружия (1 фаза)
        UpdateWeaponVisuals();

        if (Health != null)
        {
            Health.OnTakeDamage += OnBossTakeDamage;
        }

        Debug.Log($"[BossWizard] Start завершен. Фаза: Phase1, Стартовое оружие: {(_startedWithPhase2 ? "StaffPhase2" : "StaffPhase1")}, Здоровье: {Health?.CurrentHealth}/{Health?.MaxHealth}");
    }

    /// <summary>
    /// Выбор случайного визуального оружия при спавне (50% / 50%)
    /// </summary>
    private void ChooseRandomWeaponVisual()
    {
        _startedWithPhase2 = Random.Range(0, 2) == 1;

        if (_startedWithPhase2)
        {
            Debug.Log("[BossWizard] Спавн с визуальным оружием: Staff Phase 2 (босс в 1 фазе)");
        }
        else
        {
            Debug.Log("[BossWizard] Спавн с визуальным оружием: Staff Phase 1 (босс в 1 фазе)");
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Time.time - _lastElementChangeTime >= _elementChangeInterval)
        {
            ChangeToRandomElement();
            _lastElementChangeTime = Time.time;
        }

        CheckHealthForPhase2();
    }

    private void OnBossTakeDamage(int damage, GameObject attacker)
    {
        Debug.Log($"[BossWizard] Получен урон {damage}. Текущее здоровье: {Health?.CurrentHealth}/{Health?.MaxHealth}");
        CheckHealthForPhase2();
    }

    private void CheckHealthForPhase2()
    {
        if (Health == null)
        {
            Debug.LogError("[BossWizard] Health = null!");
            return;
        }

        // Если уже во 2 фазе - выходим
        if (_isPhase2)
        {
            return;
        }

        float healthPercent = (float)Health.CurrentHealth / Health.MaxHealth;
        Debug.Log($"[BossWizard] CheckHealthForPhase2: healthPercent={healthPercent:F2}, threshold={_weaponChangeHealthThreshold}");

        if (healthPercent <= _weaponChangeHealthThreshold)
        {
            SwitchToPhase2();
        }
    }

    private void SwitchToPhase2()
    {
        _isPhase2 = true;
        UpdateWeaponVisuals();  // Меняем оружие на противоположное
        Debug.Log("[BossWizard] ★★★★★ SWITCHED TO PHASE 2! ★★★★★");
        Debug.Log($"[BossWizard] Оружие изменено: {(_startedWithPhase2 ? "StaffPhase1" : "StaffPhase2")}");
    }

    private void UpdateWeaponVisuals()
    {
        if (_staffPhase1 == null || _staffPhase2 == null) return;

        // Логика смены оружия:
        // в 1 фазе: показываем выбранное при спавне оружие
        // во 2 фазе: показываем другое оружие

        if (_isPhase2)
        {
            // 2 фаза: показываем противоположное оружие
            if (_startedWithPhase2)
            {
                // Спавнился с Phase2 - во 2 фазе показываем Phase1
                _staffPhase1.SetActive(true);
                _staffPhase2.SetActive(false);
                Debug.Log("[BossWizard] 2 фаза: показываем StaffPhase1");
            }
            else
            {
                // Спавнился с Phase1 - во 2 фазе показываем Phase2
                _staffPhase1.SetActive(false);
                _staffPhase2.SetActive(true);
                Debug.Log("[BossWizard] 2 фаза: показываем StaffPhase2");
            }
        }
        else
        {
            // 1 фаза: показываем выбранное при спавне оружие
            if (_startedWithPhase2)
            {
                _staffPhase1.SetActive(false);
                _staffPhase2.SetActive(true);
                Debug.Log("[BossWizard] 1 фаза: показываем StaffPhase2 (выбранное при спавне)");
            }
            else
            {
                _staffPhase1.SetActive(true);
                _staffPhase2.SetActive(false);
                Debug.Log("[BossWizard] 1 фаза: показываем StaffPhase1 (выбранное при спавне)");
            }
        }
    }

    /// <summary>
    /// Определяет, какое оружие сейчас активно (для выбора эффекта)
    /// </summary>
    private bool IsUsingStaffPhase1()
    {
        // Phase1 оружие активно если:
        // в 1 фазе и спавнился с Phase1
        // во 2 фазе и спавнился с Phase2 (тогда поменяли на Phase1)
        return (!_isPhase2 && !_startedWithPhase2) || (_isPhase2 && _startedWithPhase2);
    }

    private void ChangeToRandomElement()
    {
        var availableElements = System.Enum.GetValues(typeof(ElementType));
        ElementType newElement;

        do
        {
            newElement = (ElementType)Random.Range(0, availableElements.Length);
        }
        while (newElement == _currentElement && availableElements.Length > 1);

        _currentElement = newElement;
        Debug.Log($"[BossWizard] Element changed to: {_currentElement}");
    }

    public void PerformNormalAttack()
    {
        if (_isAttacking) return;
        if (Time.time - _lastNormalAttackTime < _normalAttackCooldown) return;

        _isAttacking = true;
        _lastNormalAttackTime = Time.time;

        ElementalAttack attackData = GetElementalAttackData(_currentElement);
        if (attackData != null && attackData.normalProjectile != null && _singleSpawnPoint != null)
        {
            GameObject projectile = Instantiate(attackData.normalProjectile, _singleSpawnPoint.position, _singleSpawnPoint.rotation);

            // Определяем, какой эффект использовать (в зависимости от оружия)
            bool usingStaffPhase1 = IsUsingStaffPhase1();
            GameObject hitEffect = usingStaffPhase1 ? attackData.hitEffect_StaffPhase1 : attackData.hitEffect_StaffPhase2;

            // Передаем эффект в снаряд
            BossProjectile projectileScript = projectile.GetComponent<BossProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(hitEffect);
            }

            Debug.Log($"[BossWizard] Normal Attack: {_currentElement}, Оружие: {(usingStaffPhase1 ? "StaffPhase1" : "StaffPhase2")}");
        }

        Invoke(nameof(ResetAttackFlag), AttackDuration);
    }

    public void PerformStrongAttack()
    {
        if (_isAttacking) return;
        if (Time.time - _lastStrongAttackTime < _strongAttackCooldown) return;

        _isAttacking = true;
        _lastStrongAttackTime = Time.time;

        ElementalAttack attackData = GetElementalAttackData(_currentElement);
        if (attackData != null && attackData.strongProjectile != null && _doubleSpawnPoints != null && _doubleSpawnPoints.Length >= 2)
        {
            foreach (var spawnPoint in _doubleSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    GameObject projectile = Instantiate(attackData.strongProjectile, spawnPoint.position, spawnPoint.rotation);

                    // Определяем, какой эффект использовать (в зависимости от оружия)
                    bool usingStaffPhase1 = IsUsingStaffPhase1();
                    GameObject hitEffect = usingStaffPhase1 ? attackData.hitEffect_StaffPhase1 : attackData.hitEffect_StaffPhase2;

                    // Передаем эффект в снаряд
                    BossProjectile projectileScript = projectile.GetComponent<BossProjectile>();
                    if (projectileScript != null)
                    {
                        projectileScript.Initialize(hitEffect);
                    }
                }
            }
            Debug.Log($"[BossWizard] Strong Attack: {_currentElement}, Оружие: {(IsUsingStaffPhase1() ? "StaffPhase1" : "StaffPhase2")}");
        }

        Invoke(nameof(ResetAttackFlag), AttackDuration);
    }

    private ElementalAttack GetElementalAttackData(ElementType element)
    {
        if (_elementalAttacks == null) return null;

        foreach (var attack in _elementalAttacks)
        {
            if (attack.elementType == element)
            {
                return attack;
            }
        }
        return null;
    }

    private void ResetAttackFlag()
    {
        _isAttacking = false;
    }

    private GameObject GetProjectileForElement(ElementType element, bool isStrong)
    {
        ElementalAttack attackData = GetElementalAttackData(element);
        if (attackData == null) return null;

        return isStrong ? attackData.strongProjectile : attackData.normalProjectile;
    }

    public bool CanPerformStrongAttack()
    {
        return Time.time - _lastStrongAttackTime >= _strongAttackCooldown && !_isAttacking;
    }

    public bool CanPerformNormalAttack()
    {
        return Time.time - _lastNormalAttackTime >= _normalAttackCooldown && !_isAttacking;
    }

    public bool IsInPhase2()
    {
        return _isPhase2;
    }

    public void SetPhase(bool isPhase2)
    {
        _isPhase2 = isPhase2;
        UpdateWeaponVisuals();

        if (_isPhase2 && Health != null)
        {
            int maxHealth = Health.MaxHealth;
            int currentHealth = Health.CurrentHealth;
            int phase2HealthThreshold = Mathf.FloorToInt(maxHealth * _weaponChangeHealthThreshold);

            if (currentHealth > phase2HealthThreshold)
            {
                Health.SetHealth(phase2HealthThreshold);
            }
        }

        Debug.Log($"[BossWizard] Phase restored: {(_isPhase2 ? "Phase2" : "Phase1")}");
    }

    public void ResetPhaseState()
    {
        if (Health == null) return;

        float healthPercent = (float)Health.CurrentHealth / Health.MaxHealth;

        _isPhase2 = healthPercent <= _weaponChangeHealthThreshold;
        UpdateWeaponVisuals();

        Debug.Log($"[BossWizard] ResetPhaseState: healthPercent={healthPercent:F2}, _isPhase2={_isPhase2}");
    }

    private void OnDestroy()
    {
        if (Health != null)
            Health.OnTakeDamage -= OnBossTakeDamage;
    }
}
