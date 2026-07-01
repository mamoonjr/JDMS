-- JDMS: محافظات ومناطق المملكة الأردنية (12 محافظة، 127 منطقة)
-- يُنفّذ تلقائياً عند تشغيل التطبيق من JordanLocationSeedData.cs
-- يضيف المحافظات/المناطق الناقصة فقط (لا يحذف القديم ولا يكرر الاسم).
-- USE jdms_db;

SELECT 'لتعبئة البيانات: أعد تشغيل التطبيق (dotnet run) وسيتم دمج المناطق الناقصة تلقائياً.' AS Info;

SELECT g.NameAr AS المحافظة, COUNT(a.Id) AS عدد_المناطق
FROM Governorates g
LEFT JOIN Areas a ON a.GovernorateId = g.Id
GROUP BY g.Id, g.NameAr
ORDER BY g.NameAr;
