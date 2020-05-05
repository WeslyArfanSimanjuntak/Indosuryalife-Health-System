-- Delete all data referenced to policy


DECLARE @policyId VARCHAR(50);

SET @policyId = '0000000022'

DELETE
FROM MemberPlan_H where PolicyId = @policyId;

DELETE
FROM pcf
WHERE PolicyId = @policyId;

DELETE
FROM PCF_Endorse
WHERE PolicyId = @policyId;

--DELETE
--FROM PlanDetail
--WHERE PolicyId = @policyId;
DELETE
FROM PlanDetail_Endorse
WHERE PolicyId = @policyId;

DELETE
FROM MemberPlan
WHERE PolicyId = @policyId;

DELETE
FROM MemberPlan_Endorse
WHERE PolicyId = @policyId;

--DELETE
--FROM [Plan]
--WHERE PolicyId = @policyId;
DELETE
FROM Member_Movement_Client
WHERE MovementId IN (
		SELECT Id
		FROM Member_Movement
		WHERE PolicyId = @policyId
		)

DELETE
FROM Member_Movement
WHERE  PolicyId = @policyId
		

DELETE
FROM Member
WHERE PolicyId = @policyId;

DELETE
FROM Member_Endorse
WHERE PolicyId = @policyId;

DELETE
FROM FinanceTransactionDetail
WHERE TransactionNumber IN (
		SELECT TransactionNumber
		FROM FinanceTransaction
		WHERE PolicyId = @policyId
		);

DELETE
FROM FinanceTransaction
WHERE PolicyId = @policyId;

DELETE
FROM FinanceTransaction
WHERE PolicyId = @policyId;

DELETE
FROM Policy_Endorse
WHERE PolicyId = @policyId;

DELETE
FROM Plan_Endorse
WHERE PolicyId = @policyId;

DELETE
FROM Endorsement
WHERE PolicyId = @policyId;

update Policy set PolicyStatus = 'Inactive', PolicyNumber = null
	WHERE PolicyId = @policyId

	--DELETE
	--FROM [Policy]
	--WHERE PolicyId = @policyId;
	--SELECT *
	--FROM Member
	--WHERE PolicyId = '0000000010'
	--DELETE client
	--WHERE ClientId NOT IN (
	--		SELECT Client.ClientId
	--		FROM Client
	--		INNER JOIN Member ON Member.ClientId = Client.ClientId
	--		WHERE PolicyId = '0000000011'
	--			OR PolicyId = '0000000009'
	--			OR Client.Type = 'Company'
	--		) and Type ='Personal' and RelateTo is null
	--delete
	--FROM client
	--WHERE ClientId NOT IN (
	--		SELECT Client.ClientId
	--		FROM Client
	--		INNER JOIN Member ON Member.ClientId = Client.ClientId
	--		WHERE PolicyId = '0000000011'
	--			OR PolicyId = '0000000009'
	--			OR Client.Type = 'Company'
	--		) and Type ='Personal' and  ClientId not in(
	--		select distinct(ContactPerson) from client where ContactPerson is not null)
	-- Find all table names with column name
	--SELECT c.name AS ColName
	--	,t.name AS TableName
	--FROM sys.columns c
	--INNER JOIN sys.tables t ON c.object_id = t.object_id
	--WHERE c.name LIKE '%EndorseNumber%';
	--SELECT member.MemberNumber
	--	,pcf.*
	--FROM pcf
	--LEFT JOIN member ON member.MemberId = pcf.MemberId
	--WHERE member.MemberNumber = '0000138'
	--SELECT Member_Endorse.MemberNumber
	--	,PCF_Endorse.*
	--FROM PCF_Endorse
	--LEFT JOIN Member_Endorse ON PCF_Endorse.MemberId = PCF_Endorse.MemberId
	--WHERE Member_Endorse.MemberNumber = '0000138' and PCF_Endorse.EndorseNumber = '0000000035'
	--order by Amount
	--select * from PCF_Endorse WHERE MemberId = 39 and PCF_Endorse.EndorseNumber = '0000000035'
	--select * from Member where MemberNumber = '0000147'
	--select * from Member_Movement order by CreatedDate desc
	--select * from FinanceTransaction where EffectiveDate >= '2020-03-01' and  EffectiveDate <= '2020-03-31'
	--select * from PCF where TransactionNumber = 'TXTR-2020-000027'
	--sel


	

---- disable all constraint
--USE ISL_HEALTH_MIGRATION
--EXEC sp_MSforeachtable @command1="ALTER TABLE ? NOCHECK CONSTRAINT ALL"
--GO
--EXEC sp_MSforeachtable @command1="ALTER TABLE ? DISABLE TRIGGER ALL"
--GO


---- enable all constraints

--EXEC sp_MSforeachtable @command1="ALTER TABLE ? ENABLE TRIGGER ALL"
--GO

--EXEC sp_MSforeachtable @command1="ALTER TABLE ? CHECK CONSTRAINT ALL"
--GO