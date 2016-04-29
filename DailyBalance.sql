







--- 04/10/2011 DAE  


-- Desc.: Get all students register today who have balance > 0 


--          because this process will be run after midnight we use GETDATE function minus 1 day  


 


ALTER function [dbo].[XArBalIDs_DAILY](@id varchar(10),@term varchar(10)  


--,@date DATETIME 


 )  


returns @XArBalIDs table( 


    AR_STUDENTS_ID varchar(10), 


    AR_TERM  varchar(10)) 


    AS 


BEGIN 


    insert @XArBalIDs 


 


SELECT DISTINCT 


SUBSTRING (AR_ACCTS_ID, 1, 7),  --+ ','  --AS 'ARID' 


--STC_TERM 


AR_INVOICES.INV_TERM 


 


FROM dbo.AR_ACCTS 


 INNER JOIN AR_INVOICES 


       ON SUBSTRING (AR_ACCTS.AR_ACCTS_ID, 1, 7) = INV_PERSON_ID 


       --AND AR_INVOICES.INV_TERM = '2010SP' 


       inner join STUDENT_ACAD_CRED 


       on SUBSTRING (AR_ACCTS.AR_ACCTS_ID, 1, 7) = STUDENT_ACAD_CRED.STC_PERSON_ID 


         and AR_INVOICES.INV_TERM = STC_TERM 


        INNER JOIN STC_STATUSES ON STUDENT_ACAD_CRED.STUDENT_ACAD_CRED_ID =  


                 STC_STATUSES.STUDENT_ACAD_CRED_ID 


 


 


WHERE  


SUBSTRING (AR_ACCTS.AR_ACCTS_ID, 1, 7) = COALESCE(@id , SUBSTRING (AR_ACCTS.AR_ACCTS_ID, 1, 7))  


AND RIGHT(AR_ACCTS_ID,3) = 'STC' 


--and  AR_INVOICES.INV_TERM = COALESCE(@term, AR_INVOICES.INV_TERM) AND AR_INVOICES.INV_TERM = @term 


and  AR_INVOICES.INV_TERM = @term 


and STC_TERM = @term 


--AND POS = '1' 


AND STC_STATUS IN ('A','N') 


AND CONVERT(varchar, STC_STATUS_DATE, 101) = convert(varchar,dateadd(day,-1 ,getdate()),101) --DATEADD(day, -100, GETDATE()) 


GROUP BY AR_ACCTS_ID, AR_INVOICES.INV_TERM    --STC_TERM  -- 


 


RETURN 


 


END 


 


 


 


GO

