import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,ResponsiveContainer } from 'recharts';
import style from "./SimpleAreaChart.module.css"




const SimpleAreaChart = ({data}) => {
  return (
 <div className={style.chart}>

      {}
      <ResponsiveContainer width="100%" height="100%">

        {}
        <AreaChart
        className={`${style.Innerchart}`}
          data={data}
          margin={{ top: 20, right: 10, left: -20, bottom: 0 }}
        >
          <CartesianGrid strokeDasharray="10 10" />
          <XAxis dataKey="name" />
          <YAxis domain={[0, 100]} />
          <YAxis tickCount={5} />
          <Tooltip />
          <Area type="monotone" dataKey="uv" stroke="#8884d8" fill="#8884d8" />
        </AreaChart>

      </ResponsiveContainer>
    </div>
  );
};

export default SimpleAreaChart;