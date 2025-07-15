import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
  Divider,
  Switch,
  FormControlLabel,
  Slider,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Tooltip,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Avatar,
  Badge,
} from '@mui/material';
import {
  Calculate as CalculateIcon,
  Percent as PercentIcon,
  Money as MoneyIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Settings as SettingsIcon,
  Preview as PreviewIcon,
  TrendingDown as DiscountIcon,
  Assessment as ReportIcon,
  Group as GroupIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface FamilyMember {
  id: number;
  studentId: number;
  studentName: string;
  grade: string;
  routeId: number;
  routeName: string;
  basePrice: number;
  currentDiscount: number;
  finalPrice: number;
}

interface DiscountRule {
  id: number;
  name: string;
  description: string;
  type: 'Percentage' | 'FixedAmount' | 'Tiered';
  value: number;
  minChildren: number;
  maxChildren?: number;
  isActive: boolean;
  priority: number;
  conditions: DiscountCondition[];
}

interface DiscountCondition {
  id: number;
  type: 'Grade' | 'Route' | 'PaymentMethod' | 'Duration';
  operator: 'Equals' | 'GreaterThan' | 'LessThan' | 'Contains';
  value: string;
}

interface FamilyDiscountCalculation {
  familyId: number;
  parentName: string;
  totalChildren: number;
  members: FamilyMember[];
  appliedRules: AppliedDiscountRule[];
  subtotal: number;
  totalDiscount: number;
  finalTotal: number;
  savings: number;
  savingsPercentage: number;
}

interface AppliedDiscountRule {
  ruleId: number;
  ruleName: string;
  discountAmount: number;
  affectedMembers: number[];
}

interface DiscountSimulation {
  scenario: string;
  originalTotal: number;
  discountedTotal: number;
  totalSavings: number;
  affectedFamilies: number;
  projectedRevenue: number;
}

const FamilyDiscountCalculator: React.FC = () => {
  const [families, setFamilies] = useState<FamilyDiscountCalculation[]>([]);
  const [discountRules, setDiscountRules] = useState<DiscountRule[]>([]);
  const [selectedFamily, setSelectedFamily] =
    useState<FamilyDiscountCalculation | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [calculatorDialogOpen, setCalculatorDialogOpen] = useState(false);
  const [ruleDialogOpen, setRuleDialogOpen] = useState(false);
  const [simulationDialogOpen, setSimulationDialogOpen] = useState(false);
  const [selectedRule, setSelectedRule] = useState<DiscountRule | null>(null);
  const [activeStep, setActiveStep] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterMinChildren, setFilterMinChildren] = useState(1);
  const [simulationResults, setSimulationResults] = useState<
    DiscountSimulation[]
  >([]);

  const [newRule, setNewRule] = useState<Partial<DiscountRule>>({
    name: '',
    description: '',
    type: 'Percentage',
    value: 0,
    minChildren: 2,
    isActive: true,
    priority: 1,
    conditions: [],
  });

  useEffect(() => {
    loadFamilyDiscounts();
    loadDiscountRules();
  }, []);

  const loadFamilyDiscounts = async () => {
    try {
      setLoading(true);
      const response = (await apiClient.get('/api/discounts/families')) as {
        data: FamilyDiscountCalculation[];
      };
      setFamilies(response.data || []);
    } catch {
      setError('Failed to load family discounts');
    } finally {
      setLoading(false);
    }
  };

  const loadDiscountRules = async () => {
    try {
      const response = (await apiClient.get('/api/discounts/rules')) as {
        data: DiscountRule[];
      };
      setDiscountRules(response.data || []);
    } catch {
      setError('Failed to load discount rules');
    }
  };

  const calculateFamilyDiscount = async (familyId: number) => {
    try {
      setLoading(true);
      const response = (await apiClient.post(
        `/api/discounts/calculate/${familyId}`
      )) as { data: FamilyDiscountCalculation };

      if (response.data) {
        setSelectedFamily(response.data);
        setCalculatorDialogOpen(true);
        setFamilies(prev =>
          prev.map(family =>
            family.familyId === familyId ? response.data : family
          )
        );
      }
    } catch {
      setError('Failed to calculate family discount');
    } finally {
      setLoading(false);
    }
  };

  const saveDiscountRule = async () => {
    try {
      setLoading(true);
      const endpoint = selectedRule
        ? `/api/discounts/rules/${selectedRule.id}`
        : '/api/discounts/rules';
      const method = selectedRule ? 'put' : 'post';

      const response = (await apiClient[method](endpoint, newRule)) as {
        data: { success: boolean };
      };

      if (response.data.success) {
        setSuccess(
          `Discount rule ${selectedRule ? 'updated' : 'created'} successfully`
        );
        loadDiscountRules();
        setRuleDialogOpen(false);
        resetRuleForm();
      }
    } catch {
      setError('Failed to save discount rule');
    } finally {
      setLoading(false);
    }
  };

  const deleteDiscountRule = async (ruleId: number) => {
    try {
      setLoading(true);
      const response = (await apiClient.delete(
        `/api/discounts/rules/${ruleId}`
      )) as { data: { success: boolean } };

      if (response.data.success) {
        setSuccess('Discount rule deleted successfully');
        loadDiscountRules();
      }
    } catch {
      setError('Failed to delete discount rule');
    } finally {
      setLoading(false);
    }
  };

  const runDiscountSimulation = async () => {
    try {
      setLoading(true);
      const response = (await apiClient.post('/api/discounts/simulate', {
        rules: discountRules.filter(rule => rule.isActive),
      })) as { data: DiscountSimulation[] };

      setSimulationResults(response.data || []);
      setSimulationDialogOpen(true);
    } catch {
      setError('Failed to run discount simulation');
    } finally {
      setLoading(false);
    }
  };

  const applyBulkDiscounts = async () => {
    try {
      setLoading(true);
      const response = (await apiClient.post('/api/discounts/apply-bulk')) as {
        data: { success: boolean; affectedFamilies: number };
      };

      if (response.data.success) {
        setSuccess(
          `Bulk discounts applied to ${response.data.affectedFamilies} families`
        );
        loadFamilyDiscounts();
      }
    } catch {
      setError('Failed to apply bulk discounts');
    } finally {
      setLoading(false);
    }
  };

  const resetRuleForm = () => {
    setNewRule({
      name: '',
      description: '',
      type: 'Percentage',
      value: 0,
      minChildren: 2,
      isActive: true,
      priority: 1,
      conditions: [],
    });
    setSelectedRule(null);
    setActiveStep(0);
  };

  const addCondition = () => {
    setNewRule(prev => ({
      ...prev,
      conditions: [
        ...(prev.conditions || []),
        {
          id: Date.now(),
          type: 'Grade',
          operator: 'Equals',
          value: '',
        },
      ],
    }));
  };

  const removeCondition = (conditionId: number) => {
    setNewRule(prev => ({
      ...prev,
      conditions: prev.conditions?.filter(c => c.id !== conditionId) || [],
    }));
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const getDiscountTypeColor = (type: string) => {
    switch (type) {
      case 'Percentage':
        return 'primary';
      case 'FixedAmount':
        return 'success';
      case 'Tiered':
        return 'warning';
      default:
        return 'default';
    }
  };

  const filteredFamilies = families.filter(family => {
    const matchesSearch = family.parentName
      .toLowerCase()
      .includes(searchTerm.toLowerCase());
    const matchesChildren = family.totalChildren >= filterMinChildren;
    return matchesSearch && matchesChildren;
  });

  const totalSavings = families.reduce(
    (sum, family) => sum + family.savings,
    0
  );
  const averageSavings =
    families.length > 0 ? totalSavings / families.length : 0;
  const familiesWithDiscounts = families.filter(
    family => family.totalDiscount > 0
  ).length;

  const steps = [
    'Basic Information',
    'Discount Configuration',
    'Conditions & Rules',
    'Review & Save',
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4" gutterBottom>
          Family Discount Calculator
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => {
              resetRuleForm();
              setRuleDialogOpen(true);
            }}
          >
            New Rule
          </Button>
          <Button
            variant="outlined"
            startIcon={<PreviewIcon />}
            onClick={runDiscountSimulation}
          >
            Simulate
          </Button>
          <Button
            variant="outlined"
            startIcon={<CalculateIcon />}
            onClick={applyBulkDiscounts}
          >
            Apply Bulk
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert
          severity="success"
          sx={{ mb: 2 }}
          onClose={() => setSuccess(null)}
        >
          {success}
        </Alert>
      )}

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h6">{families.length}</Typography>
              <Typography variant="body2" color="textSecondary">
                Total Families
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <DiscountIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">{familiesWithDiscounts}</Typography>
              <Typography variant="body2" color="textSecondary">
                With Discounts
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <MoneyIcon color="warning" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {formatCurrency(totalSavings)}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Total Savings
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <PercentIcon color="info" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h6">
                {formatCurrency(averageSavings)}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Average Savings
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Discount Rules Management */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            <SettingsIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
            Active Discount Rules (
            {discountRules.filter(rule => rule.isActive).length})
          </Typography>

          <List>
            {discountRules
              .filter(rule => rule.isActive)
              .map(rule => (
                <ListItem key={rule.id}>
                  <ListItemText
                    primary={rule.name}
                    secondary={
                      <Box>
                        <Typography variant="body2" color="textSecondary">
                          {rule.description}
                        </Typography>
                        <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
                          <Chip
                            label={rule.type}
                            color={getDiscountTypeColor(rule.type) as any}
                            size="small"
                          />
                          <Chip
                            label={`${rule.minChildren}+ children`}
                            size="small"
                            variant="outlined"
                          />
                          <Chip
                            label={
                              rule.type === 'Percentage'
                                ? `${rule.value}%`
                                : formatCurrency(rule.value)
                            }
                            size="small"
                            color="success"
                          />
                        </Box>
                      </Box>
                    }
                  />
                  <ListItemSecondaryAction>
                    <Tooltip title="Edit Rule">
                      <IconButton
                        onClick={() => {
                          setSelectedRule(rule);
                          setNewRule(rule);
                          setRuleDialogOpen(true);
                        }}
                      >
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Delete Rule">
                      <IconButton
                        onClick={() => deleteDiscountRule(rule.id)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>
                  </ListItemSecondaryAction>
                </ListItem>
              ))}
          </List>
        </CardContent>
      </Card>

      {/* Family Discounts Table */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              mb: 2,
            }}
          >
            <Typography variant="h6">
              Family Discount Calculations ({filteredFamilies.length})
            </Typography>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                size="small"
                label="Search families..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
              />
              <TextField
                size="small"
                label="Min Children"
                type="number"
                value={filterMinChildren}
                onChange={e => setFilterMinChildren(parseInt(e.target.value))}
                inputProps={{ min: 1, max: 10 }}
                sx={{ width: 120 }}
              />
            </Box>
          </Box>

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Parent Name</TableCell>
                    <TableCell>Children</TableCell>
                    <TableCell>Subtotal</TableCell>
                    <TableCell>Discount</TableCell>
                    <TableCell>Final Total</TableCell>
                    <TableCell>Savings</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredFamilies.map(family => (
                    <TableRow key={family.familyId}>
                      <TableCell>
                        <Box
                          sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                        >
                          <Avatar></Avatar>
                          {family.parentName}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Badge
                          badgeContent={family.totalChildren}
                          color="primary"
                        >
                          <GroupIcon />
                        </Badge>
                      </TableCell>
                      <TableCell>{formatCurrency(family.subtotal)}</TableCell>
                      <TableCell>
                        <Chip
                          label={formatCurrency(family.totalDiscount)}
                          color="success"
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body1" fontWeight="bold">
                          {formatCurrency(family.finalTotal)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Box>
                          <Typography variant="body2" color="success.main">
                            {formatCurrency(family.savings)}
                          </Typography>
                          <Typography variant="caption" color="textSecondary">
                            ({family.savingsPercentage.toFixed(1)}%)
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Tooltip title="Calculate Discount">
                          <IconButton
                            size="small"
                            onClick={() =>
                              calculateFamilyDiscount(family.familyId)
                            }
                          >
                            <CalculateIcon />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Family Discount Calculator Dialog */}
      <Dialog
        open={calculatorDialogOpen}
        onClose={() => setCalculatorDialogOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          Family Discount Calculation
          {selectedFamily && (
            <Typography variant="subtitle1" color="textSecondary">
              {selectedFamily.parentName} - {selectedFamily.totalChildren}{' '}
              Children
            </Typography>
          )}
        </DialogTitle>
        <DialogContent>
          {selectedFamily && (
            <Box>
              <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
                Family Members
              </Typography>
              <TableContainer component={Paper} sx={{ mb: 3 }}>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Student</TableCell>
                      <TableCell>Grade</TableCell>
                      <TableCell>Route</TableCell>
                      <TableCell>Base Price</TableCell>
                      <TableCell>Discount</TableCell>
                      <TableCell>Final Price</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {selectedFamily.members.map(member => (
                      <TableRow key={member.id}>
                        <TableCell>{member.studentName}</TableCell>
                        <TableCell>{member.grade}</TableCell>
                        <TableCell>{member.routeName}</TableCell>
                        <TableCell>
                          {formatCurrency(member.basePrice)}
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={formatCurrency(member.currentDiscount)}
                            color="success"
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography fontWeight="bold">
                            {formatCurrency(member.finalPrice)}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>

              <Typography variant="h6" gutterBottom>
                Applied Discount Rules
              </Typography>
              <List>
                {selectedFamily.appliedRules.map((appliedRule, index) => (
                  <ListItem key={index}>
                    <ListItemText
                      primary={appliedRule.ruleName}
                      secondary={`Discount: ${formatCurrency(appliedRule.discountAmount)} • Affected: ${appliedRule.affectedMembers.length} members`}
                    />
                  </ListItem>
                ))}
              </List>

              <Divider sx={{ my: 2 }} />
              <Grid container spacing={3}>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Subtotal
                  </Typography>
                  <Typography variant="h6">
                    {formatCurrency(selectedFamily.subtotal)}
                  </Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Total Discount
                  </Typography>
                  <Typography variant="h6" color="success.main">
                    -{formatCurrency(selectedFamily.totalDiscount)}
                  </Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Final Total
                  </Typography>
                  <Typography variant="h6" color="primary">
                    {formatCurrency(selectedFamily.finalTotal)}
                  </Typography>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Savings
                  </Typography>
                  <Typography variant="h6" color="success.main">
                    {formatCurrency(selectedFamily.savings)} (
                    {selectedFamily.savingsPercentage.toFixed(1)}%)
                  </Typography>
                </Grid>
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCalculatorDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Discount Rule Creation/Edit Dialog */}
      <Dialog
        open={ruleDialogOpen}
        onClose={() => setRuleDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          {selectedRule ? 'Edit Discount Rule' : 'Create New Discount Rule'}
        </DialogTitle>
        <DialogContent>
          <Stepper activeStep={activeStep} orientation="vertical">
            {steps.map((label, index) => (
              <Step key={label}>
                <StepLabel>{label}</StepLabel>
                <StepContent>
                  {index === 0 && (
                    <Grid container spacing={3}>
                      <Grid size={{ xs: 12 }}>
                        <TextField
                          fullWidth
                          label="Rule Name"
                          value={newRule.name || ''}
                          onChange={e =>
                            setNewRule(prev => ({
                              ...prev,
                              name: e.target.value,
                            }))
                          }
                          required
                        />
                      </Grid>
                      <Grid size={{ xs: 12 }}>
                        <TextField
                          fullWidth
                          label="Description"
                          multiline
                          rows={3}
                          value={newRule.description || ''}
                          onChange={e =>
                            setNewRule(prev => ({
                              ...prev,
                              description: e.target.value,
                            }))
                          }
                        />
                      </Grid>
                      <Grid size={{ xs: 12 }}>
                        <FormControlLabel
                          control={
                            <Switch
                              checked={newRule.isActive || false}
                              onChange={e =>
                                setNewRule(prev => ({
                                  ...prev,
                                  isActive: e.target.checked,
                                }))
                              }
                            />
                          }
                          label="Active Rule"
                        />
                      </Grid>
                    </Grid>
                  )}

                  {index === 1 && (
                    <Grid container spacing={3}>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <FormControl fullWidth>
                          <InputLabel>Discount Type</InputLabel>
                          <Select
                            value={newRule.type || 'Percentage'}
                            onChange={e =>
                              setNewRule(prev => ({
                                ...prev,
                                type: e.target.value as any,
                              }))
                            }
                            label="Discount Type"
                          >
                            <MenuItem value="Percentage">Percentage</MenuItem>
                            <MenuItem value="FixedAmount">
                              Fixed Amount
                            </MenuItem>
                            <MenuItem value="Tiered">Tiered</MenuItem>
                          </Select>
                        </FormControl>
                      </Grid>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                          fullWidth
                          label={
                            newRule.type === 'Percentage'
                              ? 'Percentage (%)'
                              : 'Amount ($)'
                          }
                          type="number"
                          value={newRule.value || 0}
                          onChange={e =>
                            setNewRule(prev => ({
                              ...prev,
                              value: parseFloat(e.target.value),
                            }))
                          }
                          inputProps={{
                            step: newRule.type === 'Percentage' ? 1 : 0.01,
                            min: 0,
                            max:
                              newRule.type === 'Percentage' ? 100 : undefined,
                          }}
                        />
                      </Grid>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                          fullWidth
                          label="Minimum Children"
                          type="number"
                          value={newRule.minChildren || 2}
                          onChange={e =>
                            setNewRule(prev => ({
                              ...prev,
                              minChildren: parseInt(e.target.value),
                            }))
                          }
                          inputProps={{ min: 1, max: 10 }}
                        />
                      </Grid>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                          fullWidth
                          label="Maximum Children (Optional)"
                          type="number"
                          value={newRule.maxChildren || ''}
                          onChange={e =>
                            setNewRule(prev => ({
                              ...prev,
                              maxChildren: e.target.value
                                ? parseInt(e.target.value)
                                : undefined,
                            }))
                          }
                          inputProps={{ min: 1, max: 20 }}
                        />
                      </Grid>
                      <Grid size={{ xs: 12 }}>
                        <Typography variant="subtitle2" gutterBottom>
                          Priority (1 = Highest)
                        </Typography>
                        <Slider
                          value={newRule.priority || 1}
                          onChange={(e, value) =>
                            setNewRule(prev => ({
                              ...prev,
                              priority: value as number,
                            }))
                          }
                          min={1}
                          max={10}
                          marks
                          valueLabelDisplay="auto"
                        />
                      </Grid>
                    </Grid>
                  )}

                  {index === 2 && (
                    <Box>
                      <Box
                        sx={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'center',
                          mb: 2,
                        }}
                      >
                        <Typography variant="h6">Conditions</Typography>
                        <Button
                          variant="outlined"
                          startIcon={<AddIcon />}
                          onClick={addCondition}
                        >
                          Add Condition
                        </Button>
                      </Box>

                      {newRule.conditions?.map((condition, conditionIndex) => (
                        <Card key={condition.id} sx={{ mb: 2 }}>
                          <CardContent>
                            <Grid container spacing={2} alignItems="center">
                              <Grid size={{ xs: 12, sm: 3 }}>
                                <FormControl fullWidth size="small">
                                  <InputLabel>Type</InputLabel>
                                  <Select
                                    value={condition.type}
                                    onChange={e => {
                                      const updatedConditions = [
                                        ...(newRule.conditions || []),
                                      ];
                                      updatedConditions[conditionIndex].type = e
                                        .target.value as any;
                                      setNewRule(prev => ({
                                        ...prev,
                                        conditions: updatedConditions,
                                      }));
                                    }}
                                    label="Type"
                                  >
                                    <MenuItem value="Grade">Grade</MenuItem>
                                    <MenuItem value="Route">Route</MenuItem>
                                    <MenuItem value="PaymentMethod">
                                      Payment Method
                                    </MenuItem>
                                    <MenuItem value="Duration">
                                      Duration
                                    </MenuItem>
                                  </Select>
                                </FormControl>
                              </Grid>
                              <Grid size={{ xs: 12, sm: 3 }}>
                                <FormControl fullWidth size="small">
                                  <InputLabel>Operator</InputLabel>
                                  <Select
                                    value={condition.operator}
                                    onChange={e => {
                                      const updatedConditions = [
                                        ...(newRule.conditions || []),
                                      ];
                                      updatedConditions[
                                        conditionIndex
                                      ].operator = e.target.value as any;
                                      setNewRule(prev => ({
                                        ...prev,
                                        conditions: updatedConditions,
                                      }));
                                    }}
                                    label="Operator"
                                  >
                                    <MenuItem value="Equals">Equals</MenuItem>
                                    <MenuItem value="GreaterThan">
                                      Greater Than
                                    </MenuItem>
                                    <MenuItem value="LessThan">
                                      Less Than
                                    </MenuItem>
                                    <MenuItem value="Contains">
                                      Contains
                                    </MenuItem>
                                  </Select>
                                </FormControl>
                              </Grid>
                              <Grid size={{ xs: 12, sm: 4 }}>
                                <TextField
                                  fullWidth
                                  size="small"
                                  label="Value"
                                  value={condition.value}
                                  onChange={e => {
                                    const updatedConditions = [
                                      ...(newRule.conditions || []),
                                    ];
                                    updatedConditions[conditionIndex].value =
                                      e.target.value;
                                    setNewRule(prev => ({
                                      ...prev,
                                      conditions: updatedConditions,
                                    }));
                                  }}
                                />
                              </Grid>
                              <Grid size={{ xs: 12, sm: 2 }}>
                                <IconButton
                                  color="error"
                                  onClick={() => removeCondition(condition.id)}
                                >
                                  <DeleteIcon />
                                </IconButton>
                              </Grid>
                            </Grid>
                          </CardContent>
                        </Card>
                      ))}
                    </Box>
                  )}

                  {index === 3 && (
                    <Box>
                      <Typography variant="h6" gutterBottom>
                        Review Rule
                      </Typography>
                      <Grid container spacing={2}>
                        <Grid size={{ xs: 12, sm: 6 }}>
                          <Typography variant="subtitle2" color="textSecondary">
                            Name
                          </Typography>
                          <Typography variant="body1">
                            {newRule.name}
                          </Typography>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                          <Typography variant="subtitle2" color="textSecondary">
                            Type
                          </Typography>
                          <Chip
                            label={newRule.type}
                            color={
                              getDiscountTypeColor(
                                newRule.type || 'Percentage'
                              ) as any
                            }
                          />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                          <Typography variant="subtitle2" color="textSecondary">
                            Value
                          </Typography>
                          <Typography variant="body1">
                            {newRule.type === 'Percentage'
                              ? `${newRule.value}%`
                              : formatCurrency(newRule.value || 0)}
                          </Typography>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                          <Typography variant="subtitle2" color="textSecondary">
                            Children Range
                          </Typography>
                          <Typography variant="body1">
                            {newRule.minChildren}+{' '}
                            {newRule.maxChildren
                              ? `(max ${newRule.maxChildren})`
                              : ''}
                          </Typography>
                        </Grid>
                        <Grid size={{ xs: 12 }}>
                          <Typography variant="subtitle2" color="textSecondary">
                            Description
                          </Typography>
                          <Typography variant="body1">
                            {newRule.description}
                          </Typography>
                        </Grid>
                        <Grid size={{ xs: 12 }}>
                          <Typography variant="subtitle2" color="textSecondary">
                            Conditions
                          </Typography>
                          <Typography variant="body1">
                            {newRule.conditions?.length || 0} condition(s)
                            defined
                          </Typography>
                        </Grid>
                      </Grid>
                    </Box>
                  )}

                  <Box sx={{ mb: 1, mt: 2 }}>
                    <Button
                      variant="contained"
                      onClick={() => {
                        if (activeStep === steps.length - 1) {
                          saveDiscountRule();
                        } else {
                          setActiveStep(activeStep + 1);
                        }
                      }}
                      sx={{ mr: 1 }}
                    >
                      {activeStep === steps.length - 1 ? 'Save Rule' : 'Next'}
                    </Button>
                    <Button
                      disabled={activeStep === 0}
                      onClick={() => setActiveStep(activeStep - 1)}
                    >
                      Back
                    </Button>
                  </Box>
                </StepContent>
              </Step>
            ))}
          </Stepper>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRuleDialogOpen(false)}>Cancel</Button>
        </DialogActions>
      </Dialog>

      {/* Simulation Results Dialog */}
      <Dialog
        open={simulationDialogOpen}
        onClose={() => setSimulationDialogOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          <ReportIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          Discount Simulation Results
        </DialogTitle>
        <DialogContent>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Scenario</TableCell>
                  <TableCell>Original Total</TableCell>
                  <TableCell>Discounted Total</TableCell>
                  <TableCell>Total Savings</TableCell>
                  <TableCell>Affected Families</TableCell>
                  <TableCell>Projected Revenue</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {simulationResults.map((result, index) => (
                  <TableRow key={index}>
                    <TableCell>{result.scenario}</TableCell>
                    <TableCell>
                      {formatCurrency(result.originalTotal)}
                    </TableCell>
                    <TableCell>
                      {formatCurrency(result.discountedTotal)}
                    </TableCell>
                    <TableCell>
                      <Typography color="success.main">
                        {formatCurrency(result.totalSavings)}
                      </Typography>
                    </TableCell>
                    <TableCell>{result.affectedFamilies}</TableCell>
                    <TableCell>
                      <Typography color="primary">
                        {formatCurrency(result.projectedRevenue)}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSimulationDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default FamilyDiscountCalculator;
