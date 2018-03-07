using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models
{
    public class JsonApiController : Controller
    {
        public AcceptedResult Accepted(IJsonApiDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Meta is null)
            {
                document.Meta = new JsonApiMeta();
            }

            document.Meta["method"] = this.Request.Method;
            document.Meta["status"] = StatusCodes.Status202Accepted.ToString();
            return this.Accepted((object)(document));
        }

        public BadRequestObjectResult BadRequest(IJsonApiDocument document, string errorTitle = null, string errorDetail = null)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Errors is null)
            {
                document.Errors = new List<JsonApiError>();
            }

            document.Errors.Insert(
                0,
                new JsonApiError
                {
                    Status = StatusCodes.Status400BadRequest.ToString(),
                    Title = errorTitle,
                    Detail = errorDetail,
                });
            return this.BadRequest((object)(document));
        }

        public CreatedResult Created(IJsonApiDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Meta is null)
            {
                document.Meta = new JsonApiMeta();
            }

            document.Meta["method"] = this.Request.Method;
            document.Meta["status"] = StatusCodes.Status201Created.ToString();
            return this.Created(document.Links.Self, document);
        }

        public ObjectResult Forbidden(IJsonApiDocument document, string errorTitle = null, string errorDetail = null)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Errors is null)
            {
                document.Errors = new List<JsonApiError>();
            }

            document.Errors.Insert(
                0,
                new JsonApiError
                {
                    Status = StatusCodes.Status403Forbidden.ToString(),
                    Title = errorTitle,
                    Detail = errorDetail,
                });
            return this.StatusCode(StatusCodes.Status403Forbidden, document);
        }

        public ObjectResult InternalServerError(IJsonApiDocument document, string errorTitle = null, string errorDetail = null)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Errors is null)
            {
                document.Errors = new List<JsonApiError>();
            }

            document.Errors.Insert(
                0,
                new JsonApiError
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Title = errorTitle,
                    Detail = errorDetail,
                });
            return this.StatusCode(StatusCodes.Status500InternalServerError, document);
        }

        public ObjectResult MethodNotAllowed(IJsonApiDocument document, string errorTitle = null, string errorDetail = null)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Errors is null)
            {
                document.Errors = new List<JsonApiError>();
            }

            document.Errors.Insert(
                0,
                new JsonApiError
                {
                    Status = StatusCodes.Status405MethodNotAllowed.ToString(),
                    Title = errorTitle,
                    Detail = errorDetail,
                });
            return this.StatusCode(StatusCodes.Status405MethodNotAllowed, document);
        }

        public NotFoundObjectResult NotFound(IJsonApiDocument document, string errorTitle = null, string errorDetail = null)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Errors is null)
            {
                document.Errors = new List<JsonApiError>();
            }

            document.Errors.Insert(
                0,
                new JsonApiError
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Title = errorTitle,
                    Detail = errorDetail,
                });
            return this.NotFound((object)(document));
        }

        public OkObjectResult Ok(IJsonApiDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Meta is null)
            {
                document.Meta = new JsonApiMeta();
            }

            document.Meta["method"] = this.Request.Method;
            document.Meta["status"] = StatusCodes.Status200OK.ToString();
            return this.Ok((object)(document));
        }

        public ObjectResult UnprocessableEntity(IJsonApiDocument document, string errorTitle = null, string errorDetail = null)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.Errors is null)
            {
                document.Errors = new List<JsonApiError>();
            }

            document.Errors.Insert(
                0,
                new JsonApiError
                {
                    Status = StatusCodes.Status422UnprocessableEntity.ToString(),
                    Title = errorTitle,
                    Detail = errorDetail,
                });
            return this.StatusCode(StatusCodes.Status422UnprocessableEntity, document);
        }
    }
}
