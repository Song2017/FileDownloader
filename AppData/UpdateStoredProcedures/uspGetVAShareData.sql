create or replace PROCEDURE USPGETVASHAREDATA(
    IN_TENANTKEY IN NVARCHAR2 DEFAULT NULL,
    IN_VALVETABLE IN NVARCHAR2 DEFAULT NULL,
    IN_EQUIPMENTKEYS IN VARCHAR2 DEFAULT NULL,
    OUT_CURSOR OUT SYS_REFCURSOR)
IS
  /******************************************************************************
  **  File:        uspGetVAShareData
  **  Description: Get VA Share Data
  **  Returns:
  **  Params:       
  **     @QueryType
  *******************************************************************************
  **  Change History
  *******************************************************************************
  ** Ben Song 2019/10/11       init
  ** Ben Song 2019/10/11       add multiple EQUIPMENTKEY
  *******************************************************************************/
  V_REPAIRSSQL VARCHAR2(10000):= '';  
BEGIN  
  
  V_REPAIRSSQL := ' SELECT * FROM ( SELECT T.EQUIPMENTKEY, T.MOSTRECENT, T.UNIQUEKEY VALVEKEY, '
    ||'DECODE(ISDATE(DATETESTED), 0, DECODE(ISDATE(DATERECEIVED),0, SUBSTR(DATECREATE, 1, 4)||''/''|| '
    ||'SUBSTR(DATECREATE, 5, 2) ||''/''|| SUBSTR(DATECREATE, 7, 2), DATERECEIVED), DATETESTED) EFFECTIVEDATE, '
    ||' NVL(T.MAINTFOR,'''') MAINTFOR, NVL(P.QUANTITY, '''') QUANTITY, NVL(P.PARTNUMBER, '''') "PARTNUMBER", '
    ||'NVL(P.PARTNAME, '''') PARTNAME, NVL(P.WORKPERFORMED,'''' ) WORKPERFORMED FROM '|| IN_VALVETABLE
    ||' T LEFT JOIN PARTS P ON T.EQUIPMENTKEY = P.EQUIPTRACKKEY AND T.UNIQUEKEY = P.REPAIRENTRYKEY AND T.TENANTKEY = P.TENANTKEY '
    ||' WHERE T.TENANTKEY = '''|| IN_TENANTKEY || ''' AND T.EQUIPMENTKEY IN (SELECT TOKEN FROM TABLE(UFSPLITSTRING('''
    ||IN_EQUIPMENTKEYS||''', CHR(8))))) R '
    ||' ORDER BY R.EQUIPMENTKEY DESC, R.MOSTRECENT DESC, R.EFFECTIVEDATE DESC, R.VALVEKEY ';
    
  --dbms_output.put_line(V_REPAIRSSQL);
  OPEN OUT_CURSOR FOR V_REPAIRSSQL;
END;