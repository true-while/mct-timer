using Microsoft.AspNetCore.Mvc.Filters;

namespace mct_timer.Models
{
    public class EmptyFilter : IActionFilter
    {
        public EmptyFilter()
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
