(function ($) {
 "use strict";
 
	 $(".touchspin1").TouchSpin({
		buttondown_class: 'btn btn-white',
		buttonup_class: 'btn btn-white',
max:10000000,
   
	});

	$(".touchspin2").TouchSpin({
		min: 0,		
		step: 1,
        max:10000000,
		decimals:0,
		boostat: 5,
		maxboostedstep: 10,
		postfix: ' Adet ',
		buttondown_class: 'btn btn-white',
		buttonup_class: 'btn btn-white'
	});
    $(".touchspin4").TouchSpin({
        min: 0,		
        step: 0.0001,
        max:10000000,
        decimals:4,
        boostat: 5,
        maxboostedstep: 10,       
        buttondown_class: 'btn btn-white',
        buttonup_class: 'btn btn-white'
    });
	$(".touchspin3").TouchSpin({
		verticalbuttons: true,
		buttondown_class: 'btn btn-white',
		buttonup_class: 'btn btn-white'
	});


	
	
 
})(jQuery); 