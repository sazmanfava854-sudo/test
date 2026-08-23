using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

/// <summary>
/// نمونه‌های واقعی incmdocsys + Income_Fiche / Duty_Fiche — تست Accounting_Doc (ارسال 1405/05/20).
/// </summary>
public static class AccountingDocTestFixtures
{
    public sealed record IncmdocsysRow(int IncmNo, decimal Val, string Desc);

    public sealed record FicheFixture(
        string FicheNo,
        FicheCategory Category,
        decimal Payable,
        int DocTyp,
        int Branch,
        int Doc,
        int Yr,
        byte ExpectedObjOnPrice,
        string AccountingPrefix,
        int ExpectedDocRow,
        int CurrentStatus,
        string BillId,
        string PaymentId,
        string BnkAcntNo,
        string PaymentBranch,
        string RayvarzActDate,
        IncmdocsysRow[] Rows);

    public static IEnumerable<object[]> AllFixtures =>
        All.Select(f => new object[] { f });

    public static readonly FicheFixture[] All =
    [
        // --- درآمد شهرسازی (DocTyp 3) ---
        Income(
            "050533518749", 173_495_000m, branch: 205, doc: 9017,
            "9000133751567", "0017349532501", "5-8-195-21-1-0-0", status: 5,
            (1278, 3_242_897m, "عوارض آتشنشاني در هنگام صدور پروانه ساختماني"),
            (100116, 8_107_243m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
            (1272, 162_144_860m, "عوارض زيربنا (غير مسکوني)")),

        Income(
            "050433511577", 9_661_316_000m, branch: 204, doc: 909,
            "9000139851466", "0966131632524", "4-2-27-41-1-0-0", status: 5,
            (100116, 212_168_101m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
            (1239, 5_120_918_626m, "هزينه تامين پارکينگ املاک داراي شرايط خاص طبق طرح (وفق قرارداد)"),
            (1278, 84_867_241m, "عوارض آتشنشاني در هنگام صدور پروانه ساختماني"),
            (1272, 4_243_362_032m, "عوارض زيربنا (غير مسکوني)")),

        Income(
            "050833439013", 2_319_000m, branch: 682, doc: 3046,
            "1048750578000", "0000231910409", "8-6-49-2-1-0-1", status: 3,
            (401312100, 2_108_182m, "بهاي هوشمندسازي خدمات شهري"),
            (206098003, 210_818m, "ارزش افزوده")),

        // --- تهاتر مبلغ (DocTyp 14/15) ---
        TahatorAmount(
            "050133513927", -3_574_070_243m, branch: 102, doc: 2392, docTyp: 14,
            "1-4-3-2-1-0-0",
            (200098, -3_574_070_243m, "مبلغ تهاتر")),

        TahatorAmount(
            "050133513954", -90_977_801_149m, branch: 102, doc: 2393, docTyp: 14,
            "1-14-49-1-1-0-0",
            (200098, -90_977_801_149m, "مبلغ تهاتر")),

        TahatorAmount(
            "050433512973", -12_509_840_091m, branch: 102, doc: 46, docTyp: 15,
            "4-2-24-1-1-0-0",
            (200099, -12_509_840_091m, "مبلغ تهاتر")),

        // --- تهاتر درآمد (DocTyp 17/18) ---
        TahatorIncome(
            "050133513928", 3_574_070_243m, branch: 201, doc: 70, docTyp: 17,
            "1-4-3-2-1-0-0",
            (1271, 3_114_895_629m, "عوارض زيربنا (مسکوني)"),
            (100116, 166_612_250m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
            (1275, 217_349_377m, "عوارض بر بالکن و پيش آمدگي"),
            (1288, 66_644_900m, "عوارض آتشنشاني در هنگام صدور پايانکار ساختماني"),
            (1025, 8_568_087m, "جرائم کميسيون ماده 100")),

        TahatorIncome(
            "050133513009", 50_972_901_066m, branch: 201, doc: 5, docTyp: 18,
            "1-14-18-14-1-0-0",
            (1278, 1_415_913_919m, "عوارض آتشنشاني در هنگام صدور پروانه ساختماني"),
            (100116, 2_359_856_531m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
            (1270, 47_197_130_616m, "عوارض زيربنا (مسکوني)")),

        TahatorIncome(
            "050133513955", 90_977_801_149m, branch: 201, doc: 71, docTyp: 17,
            "1-14-49-1-1-0-0",
            (1282, 17_138_092_791m, "عوارض ارزش افزوده ناشي از تغيير کاربري عرصه در اجراي طرح هاي توسعه شهري"),
            (1273, 33_224_358_680m, "عوارض زيربنا (غير مسکوني)"),
            (100116, 1_661_217_934m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
            (1026, 38_289_644_571m, "جرائم ناشي از تبصره 5 ماده 100"),
            (1288, 664_487_173m, "عوارض آتشنشاني در هنگام صدور پايانکار ساختماني")),

        TahatorIncome(
            "050433512974", 12_509_840_091m, branch: 204, doc: 1, docTyp: 18,
            "4-2-24-1-1-0-0",
            (1239, 1_661_249_320m, "هزينه تامين پارکينگ املاک داراي شرايط خاص طبق طرح (وفق قرارداد)"),
            (1278, 62_542_425m, "عوارض آتشنشاني در هنگام صدور پروانه ساختماني"),
            (100116, 156_356_063m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
            (1272, 3_127_121_256m, "عوارض زيربنا (غير مسکوني)"),
            (1282, 7_502_571_027m, "عوارض ارزش افزوده ناشي از تغيير کاربري عرصه در اجراي طرح هاي توسعه شهري")),

        TahatorIncome(
            "050733512989", 23_450_577_580m, branch: 207, doc: 2, docTyp: 18,
            "7-6-54-68-1-0-0",
            (100116, 1_085_674_888m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
            (1278, 651_404_933m, "عوارض آتشنشاني در هنگام صدور پروانه ساختماني"),
            (1274, 1_395_694_178m, "عوارض بر بالکن و پيش آمدگي"),
            (1270, 20_048_123_755m, "عوارض زيربنا (مسکوني)"),
            (1277, 269_679_826m, "عوارض مستحدثات واقع در محوطه املاک (آلاچيق ، پارکينگ مسقف و استخر)")),

        // --- نوسازی (DocTyp 1) ---
        Nosazi(
            "031105/0270324", 39_868_000m, branch: 203, doc: 1296,
            "9000111251366", "0003986812580", "3-10-20-66-0-0-0",
            (2003, 19_167_594m, "نوسازی"),
            (100002, 704_714m, "آتش نشاني"),
            (100003, 18_222_407m, "پسماند"),
            (206098003, 1_773_285m, "ماليات برارزش افزوده")),

        Nosazi(
            "801105/0355816", 44_029_000m, branch: 218, doc: 835,
            "9000123052369", "0004402912522", "80-4-28-50-0-0-0",
            (2003, 17_095_431m, "نوسازی"),
            (100003, 24_515_786m, "پسماند"),
            (206098003, 2_417_783m, "ماليات برارزش افزوده")),

        // --- صنفی (DocTyp 2) ---
        Senfi(
            "101205/20493273", 7_508_000m, branch: 210, doc: 30545,
            "9000365652069", "0000750822581", "7-14-55-1-1-0-1",
            (100062, 3_293_870m, "صنفی"),
            (100003, 3_831_027m, "پسماند"),
            (206098003, 383_103m, "ماليات برارزش افزوده")),

        Senfi(
            "101205/20493800", 1_583_000m, branch: 210, doc: 30544,
            "9000366452063", "0000158322588", "7-14-55-1-1-0-1",
            (100062, 1_009_060m, "صنفی"),
            (100003, 521_764m, "پسماند"),
            (206098003, 52_176m, "ماليات برارزش افزوده")),
    ];

    private static FicheFixture Income(
        string ficheNo, decimal payable, int branch, int doc,
        string billId, string paymentId, string bnkAcntNo, int status,
        params (int IncmNo, decimal Val, string Desc)[] rows) =>
        Build(
            ficheNo, FicheCategory.Income, payable, 3, branch, doc,
            AccountingDocRowBuilder.ObjOnPriceIncome, "Incm",
            AccountingDocRowBuilder.PhasTypeRayvarz, status,
            billId, paymentId, bnkAcntNo, "18", "14050520", rows);

    private static FicheFixture TahatorAmount(
        string ficheNo, decimal payable, int branch, int doc, int docTyp,
        string bnkAcntNo,
        params (int IncmNo, decimal Val, string Desc)[] rows) =>
        Build(
            ficheNo, FicheCategory.Income, payable, docTyp, branch, doc,
            AccountingDocRowBuilder.ObjOnPriceIncome, "Incm",
            AccountingDocRowBuilder.PhasTypeRayvarz, 3,
            ficheNo, ficheNo, bnkAcntNo, "", "14050520", rows);

    private static FicheFixture TahatorIncome(
        string ficheNo, decimal payable, int branch, int doc, int docTyp,
        string bnkAcntNo,
        params (int IncmNo, decimal Val, string Desc)[] rows) =>
        Build(
            ficheNo, FicheCategory.Income, payable, docTyp, branch, doc,
            AccountingDocRowBuilder.ObjOnPriceIncome, "Incm",
            AccountingDocRowBuilder.PhasTypeRayvarz, 3,
            ficheNo, ficheNo, bnkAcntNo, "", "14050520", rows);

    private static FicheFixture Nosazi(
        string ficheNo, decimal payable, int branch, int doc,
        string billId, string paymentId, string bnkAcntNo,
        params (int IncmNo, decimal Val, string Desc)[] rows) =>
        Build(
            ficheNo, FicheCategory.DutyNosazi, payable, 1, branch, doc,
            AccountingDocRowBuilder.ObjOnPriceNosazi, "Nos", 1, 4,
            billId, paymentId, bnkAcntNo, "18", "14050519", rows);

    private static FicheFixture Senfi(
        string ficheNo, decimal payable, int branch, int doc,
        string billId, string paymentId, string bnkAcntNo,
        params (int IncmNo, decimal Val, string Desc)[] rows) =>
        Build(
            ficheNo, FicheCategory.DutySenfi, payable, 2, branch, doc,
            AccountingDocRowBuilder.ObjOnPriceSenfi, "Sen", 1, 1,
            billId, paymentId, bnkAcntNo, "18", "14050520", rows);

    private static FicheFixture Build(
        string ficheNo,
        FicheCategory category,
        decimal payable,
        int docTyp,
        int branch,
        int doc,
        byte expectedObjOnPrice,
        string accountingPrefix,
        int expectedDocRow,
        int currentStatus,
        string billId,
        string paymentId,
        string bnkAcntNo,
        string paymentBranch,
        string rayvarzActDate,
        (int IncmNo, decimal Val, string Desc)[] rows) =>
        new(
            ficheNo,
            category,
            payable,
            docTyp,
            branch,
            doc,
            Yr: 1405,
            expectedObjOnPrice,
            accountingPrefix,
            expectedDocRow,
            currentStatus,
            billId,
            paymentId,
            bnkAcntNo,
            paymentBranch,
            rayvarzActDate,
            rows.Select(r => new IncmdocsysRow(r.IncmNo, r.Val, r.Desc)).ToArray());

    public static FicheHeaderDto ToFicheHeader(FicheFixture fx) => new()
    {
        Category = fx.Category,
        FicheNo = fx.FicheNo,
        NidFiche = Guid.NewGuid(),
        Payable = fx.Payable,
        DocTyp = fx.DocTyp,
        BillId = fx.BillId,
        PaymentId = fx.PaymentId,
        BnkAcntNo = fx.BnkAcntNo,
        PaymentBranch = fx.PaymentBranch,
        CurrentStatus = fx.CurrentStatus,
        RayvarzActDate = fx.RayvarzActDate,
        RayvarzDocDate = fx.RayvarzActDate,
        Rows = fx.Rows.Select(r => new IncmRowDto
        {
            IncmNo = r.IncmNo,
            Val = r.Val,
            IncmRowDsc = r.Desc
        }).ToList()
    };

    public static RayvarzDocMeta ToRayMeta(FicheFixture fx) => new()
    {
        Branch = fx.Branch,
        Yr = fx.Yr,
        DocTyp = fx.DocTyp,
        Doc = fx.Doc
    };
}
