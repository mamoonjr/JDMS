-- مزامنة يدوية (اختياري): التطبيق ينفّذ نفس المنطق عند التشغيل عبر EnsureJordanLocationsAsync
-- USE jdms_db;

SELECT g.NameAr AS المحافظة, COUNT(a.Id) AS عدد_المناطق, SUM(a.IsActive) AS نشطة
FROM Governorates g
LEFT JOIN Areas a ON a.GovernorateId = g.Id
GROUP BY g.Id, g.NameAr
ORDER BY g.NameAr;
