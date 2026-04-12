// Описывает методы для отображения здоровья
public interface IHealthUI
{
    /// <summary>
    /// Инициализирует UI здоровья
    /// </summary>
    /// <param name="maxHealth">Максимальное здоровье</param>
    void Initialize(int maxHealth);

    /// <summary>
    /// Обновляет отображение здоровья
    /// </summary>
    void UpdateHealth(int currentHealth, int maxHealth);

    /// <summary>
    /// Скрывает UI здоровья
    /// </summary>
    void HideHealthBar();
}