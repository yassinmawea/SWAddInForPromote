import java.util.*;
import java.util.Properties;
import java.lang.*;

import matrix.db.*;
import matrix.util.*;
import matrix.util.StringList;

import com.matrixone.apps.domain.*;
import com.matrixone.apps.domain.util.*;
import com.matrixone.apps.common.*;
import com.matrixone.apps.domain.util.MapList;

 public class ${CLASSNAME}
 {
      /**
       * Constructor.
       *
       * @param context the eMatrix <code>Context</code> object.
       * @param args holds no arguments.
       * @throws Exception if the operation fails.
       * @since EC 9.5.JCI.0.
       */

	public final String ATTR_CADTYPE = PropertyUtil.getSchemaProperty("attribute_CADType");   
	public final String ATTR_ACCESSTYPE = PropertyUtil.getSchemaProperty("attribute_AccessType");
	public final String ATTR_DESIGNATEDUSER = PropertyUtil.getSchemaProperty("attribute_DesignatedUser");
	public final String ATTR_LANGUAGE = PropertyUtil.getSchemaProperty("attribute_Language");   

	public ${CLASSNAME} (Context context, String[] args)
	  throws Exception
	{

	}
	
	public void updateINVRevisionValue(Context context, String [] args) throws Exception
	{
		int var = 0;
		
		System.out.println("Coming inside updateINVRevisionValue");
		String sObjectId = args[0];
		String strName = args[1];
		String strRevision = args[2];
		String strType = args[3];
		String strOps = args[4];
		
		DomainObject doObject;
		Map mRelatedInstance;
		MapList mlRelatedInstances;
		
		StringList slObjectList = new StringList();
		slObjectList.add("id");
		slObjectList.add("type");
		slObjectList.add("revision");
		
		if (sObjectId.equals(""))
		{
			BusinessObject bo = new BusinessObject(strType, strName, strRevision, "eService Production");
			doObject = new DomainObject(bo);
		} else{
			doObject = new DomainObject(sObjectId);
		}
		

		if (strType.equals("SW Drawing"))
		{			
			System.out.println("strType --> " + strType);
			if (strOps.equals("Checkin")) {
				System.out.println("strOps --> " + strOps);
				Map mActiveVersion = doObject.getRelatedObject(context,"Active Version", true, slObjectList, null);
				String sRevisionActiveVersion = (String) mActiveVersion.get("revision");
				System.out.println("sRevisionActiveVersion --> " + sRevisionActiveVersion);
				doObject.setAttributeValue(context, "INV_Revision", sRevisionActiveVersion);
			} else if (strOps.equals("Release")){
				System.out.println("strOps --> " + strOps);
				String sRevisionInstanceOf = (String) doObject.getInfo(context, "revision");
				System.out.println("sRevisionInstanceOf --> " + sRevisionInstanceOf);
				doObject.setAttributeValue(context, "INV_Revision", strRevision);
			}	 
		}
		else
		{
			System.out.println("strType --> " + strType);
			mlRelatedInstances = doObject.getRelatedObjects(context, "Instance Of","*", slObjectList, null, false, true, (short)1, "", null);
			
			for (int i=0; i < mlRelatedInstances.size(); i++)
			{
				mRelatedInstance = (Map) mlRelatedInstances.get(i);
				String sObjectIdInstanceOf = (String) mRelatedInstance.get("id");
				System.out.println("sObjectIdInstanceOf --> " + sObjectIdInstanceOf);
				DomainObject doInstanceOf = new DomainObject(sObjectIdInstanceOf);
				if (strOps.equals("Checkin")) {					
					Map mActiveVersion = doInstanceOf.getRelatedObject(context,"Active Version", true, slObjectList, null);
					String sRevisionActiveVersion = (String) mActiveVersion.get("revision");
					System.out.println("sRevisionActiveVersion --> " + sRevisionActiveVersion);
					doInstanceOf.setAttributeValue(context, "INV_Revision", sRevisionActiveVersion);
				} else if (strOps.equals("Release")){
					String sRevisionInstanceOf = (String) mRelatedInstance.get("revision");
					System.out.println("sRevisionInstanceOf --> " + sRevisionInstanceOf);
					doInstanceOf.setAttributeValue(context, "INV_Revision", sRevisionInstanceOf);
				}
			}
		}
		//return var;
	}	
	
}
  