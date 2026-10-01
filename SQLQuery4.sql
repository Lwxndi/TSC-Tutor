SELECT UserId, '[' + TscNumber + ']' AS ExactValue, LEN(TscNumber) AS Len
FROM Learners
WHERE TscNumber LIKE '%20260003%';