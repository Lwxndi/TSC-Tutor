SELECT 
    l.TscNumber,
    l.UserId AS LearnerUserId,
    lu.FirstName + ' ' + lu.LastName AS LearnerName,
    lg.ParentUserId,
    pu.FirstName + ' ' + pu.LastName AS ParentName,
    lg.RelationshipToLearner,
    lg.IsPrimaryContact
FROM Learners l
JOIN Users lu ON lu.UserId = l.UserId
LEFT JOIN LearnerGuardians lg ON lg.LearnerUserId = l.UserId
LEFT JOIN Users pu ON pu.UserId = lg.ParentUserId
WHERE l.TscNumber = 'TSC20260003'; 