$(".year").on("click", function () {
    $("#year").val(this.innerHTML);
    $("#year-form").submit();
});