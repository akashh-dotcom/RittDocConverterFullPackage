#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using BotDetect.Web;
using BotDetect.Web.Mvc;
using R2V2.Core.Publisher;
using HtmlHelper = System.Web.Mvc.HtmlHelper;

#endregion

namespace R2V2.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlString GenerateNonGoogleCaptcha(this HtmlHelper helper)
        {
            var captcha = new MvcCaptcha("Captcha");

            var webCaptcha = captcha.WebCaptcha;
            webCaptcha.UrlGenerator = CaptchaUrls.Absolute;
            webCaptcha.AddCssIncludeToBody = true;

            var textBoxString = helper.TextBox("CaptchaCode", null).ToHtmlString();
            var hiddenString = helper.Hidden("captchaErrorMessage", "Incorrect CAPTCHA code!").ToHtmlString();
            return new HtmlString($"{webCaptcha.Html}{textBoxString}{hiddenString}");
        }

        public static IHtmlString DisplayForPublisher(this HtmlHelper helper, IPublisher publisher)
        {
            var publisherName = publisher.DisplayName == null ? publisher.Name : publisher.DisplayName;
            return new HtmlString(publisherName);
        }

        public static IHtmlString Generate5StarRating(this HtmlHelper helper, int starRating)
        {
            var starImage = "<i class=\"fa fa-star star-style\" aria-hidden=\"true\"></i>";
            var html = new StringBuilder()
                .Append("<div class=\"star-wrapper\">");
            if (starRating > 0)
            {
                html.Append(starImage);
            }

            if (starRating >= 47)
            {
                html.Append(starImage);
            }

            if (starRating >= 69)
            {
                html.Append(starImage);
            }

            if (starRating >= 90)
            {
                html.Append(starImage);
            }

            if (starRating >= 97)
            {
                html.Append(starImage);
            }

            html.Append("</div>");
            return new HtmlString(html.ToString());
        }

        #region TextBoxCustom

        public static MvcHtmlString TextBoxCustom<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, bool editable)
        {
            var htmlAttributes = new Dictionary<string, object>();

            if (!editable)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            return html.TextBoxFor(expression, htmlAttributes);
        }

        public static MvcHtmlString TextBoxCustom<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, string cssClass = null, bool editable = true)
        {
            var htmlAttributes = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                htmlAttributes.Add("class", cssClass);
            }

            if (!editable)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            return html.TextBoxFor(expression, htmlAttributes);
        }

        #endregion

        #region DropDownCustom

        public static MvcHtmlString DropDownCustom<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, bool editable)
        {
            var htmlAttributes = new Dictionary<string, object>();

            if (!editable)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            return html.DropDownListFor(expression, selectList, htmlAttributes);
        }

        public static MvcHtmlString DropDownCustom<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList,
            string cssClass = null,
            bool editable = true)
        {
            var htmlAttributes = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                htmlAttributes.Add("class", cssClass);
            }

            if (!editable)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            return html.DropDownListFor(expression, selectList, htmlAttributes);
        }

        #endregion

        #region ListBoxCustom

        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        public static MvcHtmlString ListBoxCustom<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, bool editable)
        {
            var htmlAttributes = new Dictionary<string, object>();

            if (!editable)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            return html.ListBoxFor(expression, selectList, htmlAttributes);
        }

        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        public static MvcHtmlString ListBoxCustom<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList,
            string cssClass = null,
            bool editable = true)
        {
            var htmlAttributes = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                htmlAttributes.Add("class", cssClass);
            }

            if (!editable)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            return html.ListBoxFor(expression, selectList, htmlAttributes);
        }

        #endregion
    }
}