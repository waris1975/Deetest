/****** Object:  UserDefinedFunction [dbo].[X_GET_AR_ACCT_TERM_BALANCE]    Script Date: 12/9/2013 5:40:29 PM ******/ 

SET ANSI_NULLS ON 

GO 

 

SET QUOTED_IDENTIFIER ON 

GO 

-- Author : Deeqa Egal 

-- Date  : 04/02/2011 

-- Desc. : Return a student's ST type AR balance from a term I specify 

 

ALTER  function [dbo].[X_GET_AR_ACCT_TERM_BALANCE](@inArAcctsId varchar(81), @TERM varchar(10)) 

Returns decimal(12,2) 

As 

BEGIN 

    Declare @X_BALANCE decimal(12,2), 

            @X_AR_INVOICES_ID varchar(10), 

            @X_INV_DATE datetime, 

     @X_INV_TERM varchar(10), 

            @X_INV_INVOICE_ITEMS varchar(10), 

            @X_INVI_EXT_CHARGE_AMT decimal(12,2), 

            @X_INVI_EXT_CR_AMT decimal(12,2), 

            @X_INVI_AR_CODE_TAX_DISTRS varchar(10), 

            @X_ARCTD_GL_TAX_AMT decimal(12,2), 

            @X_AR_PAYMENTS_ID varchar(10), 

            @X_ARP_AMT decimal(12,2), 

            @X_ARP_DATE datetime, 

     @X_ARP_TERM varchar(10), 

            @X_ARP_REVERSAL_AMT decimal(12,2), 

            @crnt_ar_invoice_items smallint, 

            @crnt_inv_invoice_items smallint, 

            @crnt_invi_ar_code_tax_distrs smallint, 

            @loop_cntr_1 smallint, 

            @loop_cntr_2 smallint, 

            @loop_cntr_3 smallint 

 

    Set @X_BALANCE = 0 

 

    Set @crnt_ar_invoice_items = 1 

    Set @loop_cntr_1 = 0 

    Select @loop_cntr_1 = count(*) from AR_ACCTS_LS where AR_ACCTS_ID = @inArAcctsId 

    while @crnt_ar_invoice_items <= @loop_cntr_1 

    begin 

         

        Set @X_AR_INVOICES_ID = NULL 

        Set @X_AR_PAYMENTS_ID = NULL 

        Set @X_INV_DATE = NULL 

        Set @X_INV_TERM = NULL 

         

 

        Select @X_AR_INVOICES_ID = ARA_INVOICES, 

               @X_AR_PAYMENTS_ID = ARA_PAYMENTS 

        from AR_ACCTS_LS 

        where AR_ACCTS_ID = @inArAcctsId 

        and POS = @crnt_ar_invoice_items 

 

        Select @X_INV_DATE = INV_DATE, @X_INV_TERM = INV_TERM 

        from AR_INVOICES 

        where AR_INVOICES_ID = @X_AR_INVOICES_ID 

         

 

        If @TERM is null or @X_INV_TERM = @TERM 

 

        begin 

            Set @loop_cntr_2 = 0 

            Select @loop_cntr_2 = count(*) 

                  from AR_INVOICES_LS 

                  where AR_INVOICES_ID = @X_AR_INVOICES_ID 

            Set @crnt_inv_invoice_items = 1 

            while @crnt_inv_invoice_items <= @loop_cntr_2 

            begin 

                 

                Set @X_INV_INVOICE_ITEMS = NULL 

                Set @X_INVI_EXT_CHARGE_AMT = NULL 

                Set @X_INVI_EXT_CR_AMT  = null 

                 

 

                Select @X_INV_INVOICE_ITEMS = INV_INVOICE_ITEMS 

                from AR_INVOICES_LS 

                where AR_INVOICES_ID = @X_AR_INVOICES_ID 

                and POS = @crnt_inv_invoice_items 

 

                Select @X_INVI_EXT_CHARGE_AMT = INVI_EXT_CHARGE_AMT, 

                      @X_INVI_EXT_CR_AMT     = INVI_EXT_CR_AMT 

                from AR_INVOICE_ITEMS 

                where AR_INVOICE_ITEMS_ID = @X_INV_INVOICE_ITEMS 

 

                Set @X_BALANCE = isnull(@X_BALANCE,0) + isnull(@X_INVI_EXT_CHARGE_AMT,0) 

                Set @X_BALANCE = isnull(@X_BALANCE,0) - isnull(@X_INVI_EXT_CR_AMT,0) 

                Set @crnt_invi_ar_code_tax_distrs = 1 

                Set @loop_cntr_3 = 0 

                Select @loop_cntr_3 = count(*) 

                     from AR_INVOICE_ITEMS_LS 

                     where AR_INVOICE_ITEMS_ID = @X_INV_INVOICE_ITEMS 

 

                while @crnt_invi_ar_code_tax_distrs <= @loop_cntr_3 

                begin 

                     

                    Set @X_INVI_AR_CODE_TAX_DISTRS = NULL 

                    Set @X_ARCTD_GL_TAX_AMT = NULL 

                     

 

                    Select @X_INVI_AR_CODE_TAX_DISTRS = INVI_AR_CODE_TAX_DISTRS 

                        from AR_INVOICE_ITEMS_LS 

                        where AR_INVOICE_ITEMS_ID = @X_INV_INVOICE_ITEMS 

                        and POS = @crnt_invi_ar_code_tax_distrs 

                    Select @X_ARCTD_GL_TAX_AMT = ARCTD_GL_TAX_AMT 

                        from AR_CODE_TAX_GL_DISTR 

                        where AR_CODE_TAX_GL_DISTR_ID = @X_INVI_AR_CODE_TAX_DISTRS 

                    Set @X_BALANCE = isnull(@X_BALANCE,0) + isnull(@X_ARCTD_GL_TAX_AMT,0) 

                    Set @crnt_invi_ar_code_tax_distrs = @crnt_invi_ar_code_tax_distrs + 1 

                end 

            Set @crnt_inv_invoice_items  = @crnt_inv_invoice_items  + 1 

            end 

        end 

 

        If @X_AR_PAYMENTS_ID is not null 

        begin 

             

            Set @X_ARP_AMT = NULL 

            Set @X_ARP_DATE = NULL 

            Set @X_ARP_TERM = NULL 

            Set @X_ARP_REVERSAL_AMT = NULL 

             

 

            Select @X_ARP_AMT = ARP_AMT, 

                 @X_ARP_TERM = ARP_TERM,  

                 @X_ARP_DATE = ARP_DATE, 

                 @X_ARP_REVERSAL_AMT = ARP_REVERSAL_AMT 

                 from AR_PAYMENTS 

                 where AR_PAYMENTS_ID = @X_AR_PAYMENTS_ID 

 

            if @TERM is null or @X_ARP_TERM = @TERM 

            begin 

                Set @X_BALANCE = isnull(@X_BALANCE,0) - isnull(@X_ARP_AMT,0) 

                Set @X_BALANCE = isnull(@X_BALANCE,0) + isnull(@X_ARP_REVERSAL_AMT,0) 

            end 

        end 

 

        Set @crnt_ar_invoice_items = @crnt_ar_invoice_items + 1 

    end 

 

Return @X_BALANCE 

 

END 

 

GO 

 

 

 

 
