import React, { useEffect, useState } from 'react';

interface PasswordComplexityIndicatorProps {
  password: string;
  // eslint-disable-next-line no-unused-vars
  onValidationChange: (isValid: boolean) => void;
}

interface PasswordRequirement {
  label: string;
  // eslint-disable-next-line no-unused-vars
  test: (password: string) => boolean;
  met: boolean;
}

export const PasswordComplexityIndicator: React.FC<
  PasswordComplexityIndicatorProps
> = ({ password, onValidationChange }) => {
  const [requirements, setRequirements] = useState<PasswordRequirement[]>([
    {
      label: 'At least 12 characters',
      test: (password: string) => password.length >= 12,
      met: false,
    },
    {
      label: 'Contains uppercase letter (A-Z)',
      test: (password: string) => /[A-Z]/.test(password),
      met: false,
    },
    {
      label: 'Contains lowercase letter (a-z)',
      test: (password: string) => /[a-z]/.test(password),
      met: false,
    },
    {
      label: 'Contains number (0-9)',
      test: (password: string) => /[0-9]/.test(password),
      met: false,
    },
    {
      label: 'Contains special character (!@#$%^&*)',
      test: (password: string) =>
        /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(password),
      met: false,
    },
  ]);

  const [strength, setStrength] = useState<'weak' | 'medium' | 'strong'>(
    'weak'
  );

  useEffect(() => {
    const updatedRequirements = requirements.map(req => ({
      ...req,
      met: req.test(password),
    }));

    setRequirements(updatedRequirements);

    const metCount = updatedRequirements.filter(req => req.met).length;
    const allMet = metCount === requirements.length;

    let newStrength: 'weak' | 'medium' | 'strong' = 'weak';
    if (metCount >= 4) {
      newStrength = 'medium';
    }
    if (allMet) {
      newStrength = 'strong';
    }

    setStrength(newStrength);
    onValidationChange(allMet);
  }, [password, onValidationChange, requirements]);

  const getStrengthColor = () => {
    switch (strength) {
      case 'weak':
        return '#ef4444'; // red
      case 'medium':
        return '#f59e0b'; // yellow
      case 'strong':
        return '#10b981'; // green
      default:
        return '#6b7280'; // gray
    }
  };

  const getStrengthText = () => {
    switch (strength) {
      case 'weak':
        return 'Weak';
      case 'medium':
        return 'Medium';
      case 'strong':
        return 'Strong';
      default:
        return 'Very Weak';
    }
  };

  return (
    <div className="password-complexity-indicator" style={{ marginTop: '8px' }}>
      {/* Password Strength Bar */}
      <div style={{ marginBottom: '12px' }}>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: '4px',
          }}
        >
          <span style={{ fontSize: '14px', fontWeight: '500' }}>
            Password Strength:
          </span>
          <span
            style={{
              fontSize: '14px',
              fontWeight: '600',
              color: getStrengthColor(),
            }}
          >
            {getStrengthText()}
          </span>
        </div>
        <div
          style={{
            width: '100%',
            height: '6px',
            backgroundColor: '#e5e7eb',
            borderRadius: '3px',
            overflow: 'hidden',
          }}
        >
          <div
            style={{
              width: `${(requirements.filter(req => req.met).length / requirements.length) * 100}%`,
              height: '100%',
              backgroundColor: getStrengthColor(),
              transition: 'width 0.3s ease, background-color 0.3s ease',
            }}
          />
        </div>
      </div>

      {/* Requirements List */}
      <div style={{ fontSize: '13px' }}>
        <div
          style={{ marginBottom: '8px', fontWeight: '500', color: '#374151' }}
        >
          Password Requirements:
        </div>
        <ul style={{ margin: 0, paddingLeft: '16px', listStyle: 'none' }}>
          {requirements.map((requirement, index) => (
            <li
              key={index}
              style={{
                display: 'flex',
                alignItems: 'center',
                marginBottom: '4px',
                color: requirement.met ? '#10b981' : '#6b7280',
              }}
            >
              <span
                style={{
                  marginRight: '8px',
                  fontSize: '12px',
                  fontWeight: 'bold',
                }}
              >
                {requirement.met ? '✓' : '○'}
              </span>
              <span style={{ fontSize: '13px' }}>{requirement.label}</span>
            </li>
          ))}
        </ul>
      </div>

      {/* Additional Security Tips */}
      {password.length > 0 && strength !== 'strong' && (
        <div
          style={{
            marginTop: '12px',
            padding: '8px',
            backgroundColor: '#fef3c7',
            border: '1px solid #f59e0b',
            borderRadius: '4px',
            fontSize: '12px',
            color: '#92400e',
          }}
        >
          <strong>Security Tip:</strong> Use a combination of letters, numbers,
          and special characters to create a strong password that protects your
          account.
        </div>
      )}

      {/* Success Message */}
      {strength === 'strong' && (
        <div
          style={{
            marginTop: '12px',
            padding: '8px',
            backgroundColor: '#d1fae5',
            border: '1px solid #10b981',
            borderRadius: '4px',
            fontSize: '12px',
            color: '#065f46',
          }}
        >
          <strong>Excellent!</strong> Your password meets all security
          requirements.
        </div>
      )}
    </div>
  );
};

export default PasswordComplexityIndicator;
