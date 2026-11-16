// Theme ECharts “thuvien-do” cho dashboard trắng – đỏ (file 09 mục 2). Bảng màu lấy đúng file 09; chữ, lưới theo token file 03.
export const BANG_MAU_DO = ['#DC2626', '#F87171', '#7F1D1D', '#FCA5A5', '#525252', '#A3A3A3', '#B45309'];
export const FONT = "'Be Vietnam Pro', system-ui, -apple-system, 'Segoe UI', Roboto, Arial, sans-serif";

/** Thang màu đỏ cho heatmap/visualMap: --red-50, --red-300, --red-600, --red-900 (file 03). */
export const THANG_DO = ['#FEF2F2', '#FCA5A5', '#DC2626', '#7F1D1D'];

const CHU = '#5C5C5C';
const LUOI = '#ECECEC';

const truc = {
  axisLine: { lineStyle: { color: LUOI } },
  axisTick: { show: false },
  axisLabel: { color: CHU, fontFamily: FONT },
  splitLine: { lineStyle: { color: LUOI } },
  nameTextStyle: { color: CHU, fontFamily: FONT }
};

export const THEME_DO = {
  color: BANG_MAU_DO,
  backgroundColor: 'transparent',
  textStyle: { fontFamily: FONT, color: CHU },
  title: { textStyle: { color: '#1F1F1F', fontFamily: FONT, fontWeight: 600 } },
  legend: { textStyle: { color: CHU, fontFamily: FONT }, icon: 'roundRect', itemWidth: 12, itemHeight: 8 },
  tooltip: {
    backgroundColor: '#FFFFFF', borderColor: LUOI, borderWidth: 1, padding: [8, 12],
    textStyle: { color: '#1F1F1F', fontFamily: FONT, fontSize: 13 },
    extraCssText: 'box-shadow: 0 8px 24px rgba(0,0,0,.08); border-radius: 10px;'
  },
  categoryAxis: { ...truc, splitLine: { show: false } },
  valueAxis: { ...truc, axisLine: { show: false } },
  timeAxis: truc,
  // smoothMonotone 'x': đường cong không vượt quá/thấp hơn điểm dữ liệu (số lượt nguyên, thưa)
  line: { symbol: 'circle', symbolSize: 6, smooth: 0.35, smoothMonotone: 'x', lineStyle: { width: 2 } },
  bar: { itemStyle: { borderRadius: [4, 4, 0, 0] } },
  pie: { itemStyle: { borderColor: '#FFFFFF', borderWidth: 2 } },
  // Ô lịch trống: --surface-2, viền trắng
  calendar: { itemStyle: { color: '#F5F5F5', borderColor: '#FFFFFF', borderWidth: 2 }, splitLine: { show: false },
    dayLabel: { color: CHU }, monthLabel: { color: CHU } },
  dataZoom: { borderColor: LUOI, fillerColor: 'rgba(220,38,38,0.08)', handleStyle: { color: '#DC2626' }, textStyle: { color: CHU } }
};

if (window.echarts) window.echarts.registerTheme('thuvien-do', THEME_DO);
