﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FewaTelemedicine.Domain
{
    public class RouteTransformer : DynamicRouteValueTransformer
    {
        /// <summary>
        /// mapping routes when page loads
        /// where as url must contain practice and provider
        /// structure of url is> host.com/controller/action/ but for display url controller is practice and action is provider
        /// this url is used as baseHref in index.cshtml of Home Controller
        /// if url is only host name (means prctice or provider not exists in url) then 404(page not found)
        /// Added by Ajay Patil 06 Oct 2020
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (!values.ContainsKey("controller") || !values.ContainsKey("action")) return values;

            var practice = (string)values["controller"];
            var provider = (string)values["action"];
            if (practice == "Home" && provider == "Index")
            {
                return null;
            }
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var optionsBuilder = new DbContextOptionsBuilder<FewaDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

            using (var context = new FewaDbContext(optionsBuilder.Options))
            {
                var temp = (from pro in context.providers
                            join pra in context.practices on pro.practiceId equals pra.practiceId
                            where pro.url == provider && pra.url == practice
                            select pro).FirstOrDefault();

                if (temp != null)
                {
                    values["controller"] = "Home";
                    values["action"] = "Index";
                    //values["id"] = "";
                }
            }
            return values;
        }
    }
}
