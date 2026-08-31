import { useState, useEffect } from 'react';
import { supabase } from '../supabaseClient'; 

export default function BomTable() {
  const [daneBOM, setDaneBOM] = useState([]);
  const [errorMsg, setErrorMsg] = useState(null);

  useEffect(() => {
    async function pobierzDaneZChmury() {
      const { data, error } = await supabase
        .from('bom_items') 
        .select('*');

     if (error) {
        console.error('Błąd połączenia z magazynem:', error.message);
        setErrorMsg(error.message);
      } else {
        
        const posortowaneDane = data.sort((a, b) => {
          const aParts = a.structure_id.split('.').map(Number);
          const bParts = b.structure_id.split('.').map(Number);
          
          for (let i = 0; i < Math.max(aParts.length, bParts.length); i++) {
            const aVal = aParts[i] || 0;
            const bVal = bParts[i] || 0;
            if (aVal !== bVal) return aVal - bVal;
          }
          return 0;
        });

        setDaneBOM(posortowaneDane); 
        console.log('Dane posortowane i gotowe do wyświetlenia:', posortowaneDane); 
      }
    }

    pobierzDaneZChmury(); 
  }, []); 

  return (
    <div className="table-container">
      <h2>Podgląd Danych</h2>
      
      {errorMsg && <p style={{ color: 'red' }}>Błąd bazy: {errorMsg}</p>}

      {/* Ten specjalny div włącza bezpieczne przewijanie poziome, gdy tabela jest za szeroka */}
      <div className="table-scroll-wrapper">
        <table className="bom-table">
          <thead>
            <tr>
              <th>Lp_S</th>
              <th>Ilość_S</th>
              <th>Lp_P</th>
              <th>Ilość_P</th>
              <th>Typ</th>
              <th>Numer części</th>
              <th>Tytuł</th>
              <th>Dostawca</th>
              <th>Materiał_nazwa</th>
              <th>Grubość</th>
              <th>Szerokość</th>
              <th>Długość</th>
              <th>Materiał</th>
              <th>Klasa</th>
              <th>Wykończenie</th>
              <th>Kolor</th>
              <th>Masa</th>
              <th>DXF Date</th>
              <th>Miniatura</th>
              <th>Projekt</th>
            </tr>
          </thead>
          <tbody>
            {daneBOM.length === 0 ? (
              <tr>
                <td colSpan="21" style={{ textAlign: 'center' }}>Brak danych w magazynie lub czekamy na odpowiedź chmury...</td>
              </tr>
            ) : (
              daneBOM.map((part) => (
                <tr key={part.id}>
                  <td>{part.structure_id}</td>
                  <td>{part.structure_quantity}</td>
                  <td>{part.parts_id}</td>
                  <td>{part.parts_quantity}</td>
                  <td>{part.type}</td>
                  <td>{part.part_number}</td>
                  <td>{part.title}</td>
                  <td>{part.provider}</td>
                  <td>{part.material_name}</td>
                  <td>{part.thickness}</td>
                  <td>{part.size_x}</td>
                  <td>{part.size_y}</td>
                  <td>{part.mechanical_material}</td>
                  <td>{part.class}</td>
                  <td>{part.finish}</td>
                  <td>{part.color}</td>
                  <td>{part.mass}</td>
                  <td>{part.dxf_date}</td>
                  <td>{part.thumbnail}</td>
                  <td>{part.project_name}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}