namespace ManteqTask.Domain.Enums;

public static class PermissionConstants
{
    public static class Requests
    {
        public const string Create = "requests.create";
        public const string Edit = "requests.edit";
        public const string Submit = "requests.submit";
        public const string ViewOwn = "requests.view.own";
        public const string ViewAll = "requests.view.all";
        public const string Approve = "requests.approve";
        public const string Reject = "requests.reject";
    }

    public static class Audit
    {
        public const string View = "audit.view";
    }
}
