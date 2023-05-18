/*!
 * cellsByRows layout mode for Isotope
 * http://isotope.metafizzy.co
 */

( function( window ) {

'use strict';

function cellsByRowDefinition( LayoutMode ) {

  var CellsByRow = LayoutMode.create( 'cellsByRow' );

  CellsByRow.prototype._resetLayout = function() {
    var self = this;
    
    // reset properties
    this.itemIndex = 0;
  
    // wait for images to load
    imagesLoaded( this.isotope.element, function() {
      // measurements
      self.getColumnWidth();
      self.getRowHeight();
      // set cols
      self.cols = Math.floor( self.isotope.size.innerWidth / self.columnWidth );
      self.cols = Math.max( self.cols, 1 );
      // trigger Isotope layout
      self.isotope.layout();
    });
  };

  CellsByRow.prototype._getItemLayoutPosition = function( item ) {
    item.getSize();
    var col = this.itemIndex % this.cols;
    var row = Math.floor( this.itemIndex / this.cols );
    var imageWidth = item.size.outerWidth || 400;
    var imageHeight = item.size.outerHeight || 396;
    // center item within cell
    var x = ( col + 0.5 ) * this.columnWidth - imageWidth / 2;
    var y = ( row + 0.5 ) * this.rowHeight - imageHeight / 2;
    this.itemIndex++;
    return { x: x, y: y };
  };

  CellsByRow.prototype._getContainerSize = function() {
    return {
      height: Math.ceil( this.itemIndex / this.cols ) * this.rowHeight
    };
  };

  return CellsByRow;

}

if ( typeof define === 'function' && define.amd ) {
  // AMD
  define( [
      'isotope/js/layout-mode'
    ],
    cellsByRowDefinition );
} else {
  // browser global
  cellsByRowDefinition(
    window.Isotope.LayoutMode
  );
}

})( window );
