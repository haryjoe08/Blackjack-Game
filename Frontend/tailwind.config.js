/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      keyframes: {
        dealCard: {
          '0%': { opacity: '0', transform: 'translateY(-16px) scale(0.85) rotate(-4deg)' },
          '100%': { opacity: '1', transform: 'translateY(0) scale(1) rotate(0deg)' },
        },
        chipDrop: {
          '0%': { opacity: '0', transform: 'translateY(-12px) scale(0.7)' },
          '60%': { opacity: '1', transform: 'translateY(2px) scale(1.05)' },
          '100%': { transform: 'translateY(0) scale(1)' },
        },
        popIn: {
          '0%': { opacity: '0', transform: 'scale(0.9)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
        pulseGlow: {
          '0%, 100%': { boxShadow: '0 0 0 rgba(245, 214, 123, 0)' },
          '50%': { boxShadow: '0 0 18px rgba(245, 214, 123, 0.55)' },
        },
      },
      animation: {
        dealCard: 'dealCard 0.28s ease-out',
        chipDrop: 'chipDrop 0.25s ease-out',
        popIn: 'popIn 0.2s ease-out',
        pulseGlow: 'pulseGlow 1.6s ease-in-out infinite',
      },
    },
  },
  plugins: [],
}
