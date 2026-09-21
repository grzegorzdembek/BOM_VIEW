import React, { useState } from 'react';

export default function BomTable({ data, currentView }) {
  const [expandedRows, setExpandedRows] = useState(new Set());

  const toggleRow = (structureId) => {
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(structureId)) {
        next.delete(structureId);
      } else {
        next.add(structureId);
      }
      return next;
    });
  };

  const getRowClass = (type) => {
    switch (type) {
      case 'A': return 'row-type-A';
      case 'B': return 'row-type-B';
      case 'Z': 
      case 'H': return 'row-type-Z';
      case 'K': return 'row-type-K';
      default: return 'row-default';
    }
  };

  const visibleData = data.filter((item) => {
    if (currentView !== 'STRUCTURE') return true; 
    
    const parts = item.structure_id.split('.');
    if (parts.length === 1) return true; 

    let currentPath = parts[0];
    for (let i = 1; i < parts.length; i++) {
      if (!expandedRows.has(currentPath)) return false; 
      currentPath += '.' + parts[i];
    }
    return true;
  });

  return (
    <div className="table-container">
      <div className="table-scroll-wrapper">
        <table className="bom-table">
          <thead>
            <tr>
              <th>Nr</th>
              <th>Typ</th>
              <th>Numer części</th>
              <th>Nazwa</th>
              <th>Dostawca</th>
              <th>Rodzaj materiału</th>
              <th>Grubość [mm]</th>
              <th>Szerokość [mm]</th>
              <th>Długość [mm]</th>
              <th>Materiał</th>
              <th>Klasa</th>
              <th>Wykończenie</th>
              <th>Kolor</th>
              <th>Masa (Jedn.)</th>
              <th>Ilość</th>
              <th>Miniatura</th>
              <th>DXF</th>
            </tr>
          </thead>
          <tbody>
            {visibleData.length === 0 ? (
              <tr>
                <td colSpan="17" style={{ textAlign: 'center', padding: '30px' }}>
                  Brak części pasujących do wybranego widoku.
                </td>
              </tr>
            ) : (
              visibleData.map((part, index) => {
                const depth = currentView === 'STRUCTURE' ? part.structure_id.split('.').length - 1 : 0;
                
                return (
                  <tr key={part.id} className={getRowClass(part.type)}>
                    <td style={{ whiteSpace: 'nowrap' }}>
                      {currentView === 'STRUCTURE' ? (
                        <div style={{ display: 'flex', alignItems: 'center', paddingLeft: `${depth * 20}px` }}>
                          {part.type === 'A' ? (
                            <button 
                              onClick={() => toggleRow(part.structure_id)}
                              style={{ 
                                marginRight: '8px', width: '22px', height: '22px', 
                                cursor: 'pointer', background: '#334155', color: '#f8fafc', 
                                border: 'none', borderRadius: '4px', display: 'flex', 
                                alignItems: 'center', justifyContent: 'center', fontSize: '10px'
                              }}
                            >
                              {expandedRows.has(part.structure_id) ? '▼' : '▶'}
                            </button>
                          ) : (
                            <span style={{ marginRight: '8px', width: '22px', display: 'inline-block' }}></span>
                          )}
                          <span>{part.structure_id}</span>
                        </div>
                      ) : (
                        <span style={{ color: '#94a3b8' }}>{index + 1}</span>
                      )}
                    </td>
                    
                    <td>{part.type_name || '-'}</td>
                    <td style={{ fontWeight: 'bold' }}>{part.part_number}</td>
                    <td>{part.title || '-'}</td>
                    <td>{part.provider || '-'}</td>
                    <td>{part.material_name || '-'}</td>
                    
                    <td>{part.thickness > 0 ? part.thickness : '-'}</td>
                    <td>{part.size_x > 0 ? part.size_x : '-'}</td>
                    <td>{part.size_y > 0 ? part.size_y : '-'}</td>
                    
                    <td>{part.mechanical_material || '-'}</td>
                    <td>{part.class || '-'}</td>
                    <td>{part.finish || '-'}</td>
                    <td>{part.color || '-'}</td>
                    
                    <td>{part.mass > 0 ? part.mass.toFixed(2) : '-'}</td>
                    
                    <td style={{ fontWeight: 'bold', textAlign: 'center' }}>
                      {currentView === 'STRUCTURE' ? part.structure_quantity : part.parts_quantity}
                    </td>
                    
                    <td style={{ textAlign: 'center' }}>
                      {part.thumbnail ? (
                        <img 
                          src={part.thumbnail} 
                          alt="Thumb" 
                          style={{ height: '35px', objectFit: 'contain', margin: '0 auto', borderRadius: '4px' }} 
                        />
                      ) : (
                        '-'
                      )}
                    </td>
                    
                    <td style={{ textAlign: 'center' }}>
                      {part.dxf_date ? 'Gotowe' : '-'}
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}