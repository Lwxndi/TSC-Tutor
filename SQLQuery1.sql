

SELECT Id, Title, Status, OpenAt, CloseAt, AudienceType, SubjectId, GETUTCDATE() AS ServerUtcNow
FROM Assessments
WHERE IsActive = 1
ORDER BY CreatedAt DESC;