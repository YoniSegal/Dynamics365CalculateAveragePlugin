using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace AveragePlugin
{
    public class UpdateAveragePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {           
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Get a reference to the Organization service.
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            Entity participant;
            if (context.InputParameters != null)
            {
                if (context.MessageName == "Create")
                {
                    participant = (Entity)context.InputParameters["Target"];
                }
                else
                {
                    participant = context.PreEntityImages["participant_image"];
                }

                Guid guid = participant.Id;
                if (participant.Contains("new_student"))
                {
                    EntityReference studentRef = (EntityReference)participant["new_student"];
                    Guid studentId = studentRef.Id;
                    var fetchXml = $@"
                            <fetch mapping = 'logical'>
                                <entity name = 'new_participant'>
                                    <attribute name = 'new_student'/>
                                    <attribute name = 'new_grade'/>
                                        <filter type='and'>   
                                            <condition attribute='new_student' operator='eq' value='{studentId}' />   
                                        </filter> 
                                </entity>
                            </fetch>";
                    EntityCollection entities = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    int size = entities.Entities.Count;
                    double points = 0;
                    foreach (Entity e in entities.Entities)
                    {
                        points += (double)e["new_grade"];
                    }
                    double average = points / size;
                    Entity student = service.Retrieve("new_student", studentId, new ColumnSet("new_average"));
                    student["new_average"] = average;
                    service.Update(student);
                }
            }
        }
    }
}