function LiveDevices()
{

  this._top = 100;
  this._country = '';
  this._applicationID = '';
  this._from = '';
  this._to = '';

  this.init = function()
  {
    this.onSubmitClick();
  }

  this.onSubmitClick = function()
  {
    var self = this;
    $('#btnSubmit').click(function () { self.load(); });
  }

  this.collectData = function()
  {
    this._top = $('#input_top').val();
    this._country = $('#input_country').val();
    this._applicationID = $('#input_application option:selected').attr('value');
    this._from = $('#input_from').val();
    this._to = $('#input_to').val();
  }

  this.buildQuery = function()
  {
    this.collectData();
    var query = '&top=' + this._top;
    query += '&country=' + this._country;
    query += '&from=' + this._from;
    query += '&appID=' + this._applicationID;
    query += '&to=' + this._to;
    return query;
  }

  this.load = function()
  {
    var self = this;
    console.log(self.buildQuery());
    $('#data_loading').css('display', 'block');
    $.ajax({
      type: 'POST',
      data: self.buildQuery(),
      url: '/live/getDevices',
      success:function(response)
      {
        $('#data_loading').css('display', 'none');
        if (typeof response.message !== 'undefined')
          alert(response.message)
        else
          $('#data').html(response);
      }
    });
  }

  this.init();
}