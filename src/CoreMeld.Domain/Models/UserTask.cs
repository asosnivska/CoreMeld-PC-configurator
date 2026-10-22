using System.ComponentModel;

namespace CoreMeld.Domain.Models;

public enum UserTask
{
    [Description("Пошук інформації в інтернеті та робота в браузері")]
    WebBrowsing,

    [Description("Перегляд відео та фільмів у високій якості")]
    MediaConsumption,

    [Description("Зберігання великої кількості файлів")]
    FileStorage,

    [Description("Робота з офісними програмами (Word, Excel, PowerPoint)")]
    OfficeWork,

    [Description("Відеоконференції та месенджери (Zoom, Teams, Slack, Discord)")]
    VideoConferencing,

    [Description("Робота з великою кількістю вкладок та інтерактивними дошками (Miro)")]
    HeavyMultitasking,

    [Description("Програмування")]
    Programming,

    [Description("Інженерне проєктування та архітектура (AutoCAD, ArchiCAD, Revit)")]
    EngineeringAndCad,

    [Description("Математичні розрахунки та моделювання")]
    MathModeling,

    [Description("Робота зі звуком та створення музики (FL Studio, Ableton Live, Cubase)")]
    AudioProduction,

    [Description("Економіка, облік та аналітика (BAS, 1С, Power BI)")]
    EconomicsAndAnalytics,

    [Description("Гра в сучасні ігри на стандартних моніторах (Full HD)")]
    Gaming1080p,

    [Description("Гра з високою деталізацією картинки на великих екранах (2K)")]
    Gaming1440p,

    [Description("Запуск найновіших ігор на максимальних налаштуваннях (4K)")]
    Gaming4K,

    [Description("Створення UI дизайну (Figma)")]
    UiDesign,

    [Description("Малювання ілюстрацій та обробка фото (Photoshop, Illustrator)")]
    PhotoEditing,

    [Description("Монтаж відео у високій роздільній здатності (Premiere Pro, DaVinci Resolve)")]
    VideoEditing,

    [Description("Створення 3D-моделей та прорахунок їх фізики (Blender, Maya)")]
    ThreeDModeling,

    [Description("Аналіз великих масивів інформації та робота зі штучним інтелектом")]
    AiAndBigData
}