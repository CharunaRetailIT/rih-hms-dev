using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Common
{
    public static  class HMSExtensions
    {
        public static TDestination MatchAndMap<TSource, TDestination>(TSource source, TDestination destination)       
        {

            try
            {
                var typeOfA = source.GetType();
                var typeOfB = destination.GetType();
                foreach (var fieldOfA in typeOfA.GetFields())
                {
                    var fieldOfB = typeOfB.GetField(fieldOfA.Name);
                    fieldOfB.SetValue(destination, fieldOfA.GetValue(source));
                }

                var getList = typeOfA.GetProperties();

                foreach (var propertyOfA in typeOfA.GetProperties())
                {
                    if (propertyOfA.Name != "KitchenPrinters_Modl")
                    {
                        if (propertyOfA.Name != "PriceLevelTypes")
                        {
                            if (propertyOfA.Name != "PriceLevelLists")
                            {
                                if (propertyOfA.Name != "KitchenPrinters_Modl1")
                                {
                                    if (propertyOfA.Name != "ImagePath")
                                    {
                                        var propertyOfB = typeOfB.GetProperty(propertyOfA.Name);
                                        if (propertyOfA != null || propertyOfB != null)
                                        {
                                            if (propertyOfA.Name != "KitchenLocationCount")
                                            {
                                                var getValues = propertyOfA.GetValue(source);
                                                if (getValues != null && propertyOfB != null)
                                                {
                                                    try
                                                    {

                                                        propertyOfB.SetValue(destination, propertyOfA.GetValue(source));
                                                    }
                                                    catch(Exception ex)
                                                    {

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return destination;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

     

       
    }
}
