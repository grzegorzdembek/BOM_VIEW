export default function BomTable({ data, currentView }) {

  return (
    <div className="table-container">
      <div className="table-scroll-wrapper">
        <table className="bom-table">
          <thead>
            <tr>
              {currentView === 'STRUCTURE' && <th>Lp_S</th>}
              {currentView === 'STRUCTURE' && <th>Ilość_S</th>}
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
            {data.length === 0 ? (
              <tr>
                <td colSpan={currentView === 'STRUCTURE' ? "20" : "18"} style={{ textAlign: 'center', padding: '30px' }}>
                  Brak części pasujących do wybranego widoku.
                </td>
              </tr>
            ) : (
              data.map((part) => (
                <tr key={part.structure_id}>
                  {currentView === 'STRUCTURE' && <td>{part.structure_id}</td>}
                  {currentView === 'STRUCTURE' && <td>{part.structure_quantity}</td>}
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